using System.Buffers;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using DiscordGames.Core.Memory;
using DiscordGames.Core.Net.Serialize;
using DiscordGames.Grains.Interfaces;
using PooledAwait;
using WebServer.LogMessages.Net;

namespace WebServer.Net;

public partial class Connection : IDisposable
{
    private const int ReadBufferSize = 1024;
    
    private readonly ILogger<Connection> logger;
    private readonly IClusterClient cluster;

    private bool isDisposed = true;
    private WebSocket socket = default!;
    private CancellationTokenSource recvTaskCancel = default!;
    private CancellationTokenSource sendTaskCancel = default!;

    private byte[]? buffer;
    private BufferReader bufferReader;
    private Task? sendTask;
    private long lastPintSentAtUtc;

    public string Address { get; private set; } = default!;
    public UserId UserId { get; private set; }
    public long LastActiveAtUtc { get; private set; }
    public int PingMS { get; private set; }

    public Connection(ILogger<Connection> logger, IClusterClient cluster)
    {
        this.logger = logger;
        this.cluster = cluster;
    }

    public void Initialize(WebSocket webSocket, string clientAddress)
    {
        if (!this.isDisposed) CoreThrowHelper.ThrowInvalidOperation();
        
        this.socket = webSocket;
        this.recvTaskCancel = new CancellationTokenSource();
        this.sendTaskCancel = new CancellationTokenSource();
        this.buffer = ArrayPool<byte>.Shared.Rent(ReadBufferSize);
        this.bufferReader = new BufferReader(this.buffer);
        this.sendTask = null;
        this.lastPintSentAtUtc = 0;
        this.isDisposed = false;
        this.Address = clientAddress;
        this.UserId = default;
        this.LastActiveAtUtc = 0;
        this.PingMS = 0;
    }

    public void Dispose()
    {
        if (this.isDisposed) return;

        if (this.buffer != null)
        {
            ArrayPool<byte>.Shared.Return(this.buffer);
            this.buffer = null;
        }
        this.recvTaskCancel.Dispose();
        this.sendTaskCancel.Dispose();
        if (this.sendTask is { IsCompleted: false }) this.sendTask.Wait();
        
        this.isDisposed = true;
        
        GC.SuppressFinalize(this);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private async PooledValueTask Disconnect(CancellationToken cancellationToken, WebSocketCloseStatus status = WebSocketCloseStatus.NormalClosure)
    {
        await this.socket.CloseOutputAsync(status, string.Empty, cancellationToken);
    }
    
    private ValueTask PreserveSend(byte[] data)
    {
        if (this.UserId == 0) CoreThrowHelper.ThrowInvalidOperation();

        var user = this.cluster.GetGrain<IUserGrain>(this.UserId);
        return user.ReserveSend(data);
    }

    private async PooledTask ProcessSend()
    {
        while (this.socket.State is WebSocketState.Open)
        {
            if (this.sendTaskCancel.IsCancellationRequested) break;

            var grain = this.cluster.GetGrain<IUserGrain>(this.UserId);
            var sendQueue = await grain.GetAndClearQueue();

            foreach (var sendBuffer in sendQueue)
            {
                await this.socket.SendAsync(sendBuffer, WebSocketMessageType.Binary, WebSocketMessageFlags.EndOfMessage,
                    this.sendTaskCancel.Token);
            }
        }
    }

    public ValueTask Loop()
    {
        return Internal(this, this.recvTaskCancel.Token);
        static async PooledValueTask Internal(Connection self, CancellationToken cancellationToken)
        {
            self.LastActiveAtUtc = DateTime.UtcNow.Ticks;
            self.logger.LogOnConnected(self.Address);
            
            try
            {
                while (self.socket.State is WebSocketState.Open)
                {
                    try
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            await self.Disconnect(CancellationToken.None);
                            break;
                        }
                        
                        // 먼저 '읽기 영역'을 버퍼의 가장 앞쪽으로 당겨옵니다 ('쓰기 영역' 공간 확보를 위해)
                        self.bufferReader.Compact();

                        var receiveResult =
                            await self.socket.ReceiveAsync(self.bufferReader.WriteSegment, cancellationToken);

                        // 텍스트 데이터는 받지 않고, 바이트로 직렬화된 것만 받습니다
                        if (receiveResult.MessageType is WebSocketMessageType.Text)
                        {
                            self.logger.LogOnTextData(self.Address);
                            continue;
                        }

                        // 접속 종료 메시지라면 접속 해제 처리를 합니다
                        if (receiveResult.MessageType is WebSocketMessageType.Close)
                        {
                            if (self.socket.State is WebSocketState.CloseReceived)
                            {
                                await self.Disconnect(cancellationToken);
                            }
                            
                            break;
                        }

                        // 데이터 크기가 0 이하라면 바로 다음 데이터를 받기 위해 돌아갑니다
                        if (receiveResult.Count <= 0) continue;

                        // 버퍼에 데이터가 기록되었으니 '쓰기 영역'을 그 크기만큼 전진시킵니다 (이걸 실패하면 비정상이니 연결을 끊어버립니다)
                        if (!self.bufferReader.AdvanceWrite(receiveResult.Count))
                        {
                            await self.Disconnect(cancellationToken, WebSocketCloseStatus.InternalServerError);
                            break;
                        }

                        await self.bufferReader.ReadAndHandleMessage(self);

                        // 버퍼에서 데이터를 읽었으니 다음에 읽기 시작할 위치를 조정합니다 (이걸 실패하면 비정상이니 연결을 끊어버립니다)
                        if (!self.bufferReader.AdvanceRead())
                        {
                            await self.Disconnect(cancellationToken, WebSocketCloseStatus.InternalServerError);
                            break;
                        }

                        self.LastActiveAtUtc = DateTime.UtcNow.Ticks;
                    }
                    catch (OperationCanceledException) { }
                }
            }
            catch (Exception e)
            {
                self.logger.LogCritical(e, "Has exception");
            }
            finally
            {
                if (self.socket.State is not WebSocketState.Closed and not WebSocketState.Aborted) await self.Disconnect(CancellationToken.None);
                
                await self.sendTaskCancel.CancelAsync();
                if (self.sendTask != null) await self.sendTask;
                
                if (!ConnectionPool.I.Unregister(self.UserId)) CoreThrowHelper.ThrowInvalidOperation();
                
                self.logger.LogOnDisconnected(self.Address);
            }
        }
    }

    public Task Kill()
    {
        return this.recvTaskCancel.CancelAsync();
    }
}