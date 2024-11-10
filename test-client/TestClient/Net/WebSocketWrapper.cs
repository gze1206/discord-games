using System.Buffers;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using DiscordGames.Core.Memory;
using DiscordGames.Core.Net;
using DiscordGames.Core.Net.Serialize;
using PooledAwait;
using Uri = System.Uri;

namespace TestClient.Net;

public class WebSocketWrapper : IDisposable
{
    private const int ReadBufferSize = 1024;
    
    public event Action OnOpen = () => { };

    public WebSocketState State => this.socket.State;
    
    private readonly ClientWebSocket socket;
    private readonly IMessageHandler handler;

    private bool isDisposed;
    private byte[]? buffer;
    private BufferReader bufferReader;

    public WebSocketWrapper(IMessageHandler handler)
    {
        this.socket = new ClientWebSocket();
        this.handler = handler;
        this.buffer = ArrayPool<byte>.Shared.Rent(ReadBufferSize);
        this.bufferReader = new BufferReader(this.buffer);
    }
    
    public void Dispose()
    {
        if (this.isDisposed) return;
        
        if (this.buffer != null)
        {
            ArrayPool<byte>.Shared.Return(this.buffer);
            this.buffer = null;
        }
        
        this.socket.Dispose();

        this.isDisposed = true;
        GC.SuppressFinalize(this);
    }

    public async PooledValueTask Connect(Uri host, CancellationToken cancellationToken)
    {
        await this.socket.ConnectAsync(host, cancellationToken);
        OnOpen();
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public async PooledValueTask Disconnect(CancellationToken cancellationToken, WebSocketCloseStatus status = WebSocketCloseStatus.NormalClosure)
    {
        if (this.State is WebSocketState.Open or WebSocketState.CloseReceived)
            await this.socket.CloseOutputAsync(status, string.Empty, cancellationToken);
    }

    public async PooledTask ProcessReceive(CancellationToken cancellationToken)
    {
        while (this.socket.State is WebSocketState.Open)
        {
            try
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    await this.Disconnect(CancellationToken.None);
                    break;
                }

                // 먼저 '읽기 영역'을 버퍼의 가장 앞쪽으로 당겨옵니다 ('쓰기 영역' 공간 확보를 위해)
                this.bufferReader.Compact();

                var receiveResult =
                    await this.socket.ReceiveAsync(this.bufferReader.WriteSegment, cancellationToken);

                // 텍스트 데이터는 받지 않고, 바이트로 직렬화된 것만 받습니다
                if (receiveResult.MessageType is WebSocketMessageType.Text) continue;

                // 접속 종료 메시지라면 접속 해제 처리를 합니다
                if (receiveResult.MessageType is WebSocketMessageType.Close)
                {
                    await this.Disconnect(cancellationToken);
                    break;
                }

                // 데이터 크기가 0 이하라면 바로 다음 데이터를 받기 위해 돌아갑니다
                if (receiveResult.Count <= 0) continue;

                // 버퍼에 데이터가 기록되었으니 '쓰기 영역'을 그 크기만큼 전진시킵니다 (이걸 실패하면 비정상이니 연결을 끊어버립니다)
                if (!this.bufferReader.AdvanceWrite(receiveResult.Count))
                {
                    await this.Disconnect(cancellationToken, WebSocketCloseStatus.InternalServerError);
                    break;
                }

                await this.bufferReader.ReadAndHandleMessage(this.handler);

                // 버퍼에서 데이터를 읽었으니 다음에 읽기 시작할 위치를 조정합니다 (이걸 실패하면 비정상이니 연결을 끊어버립니다)
                if (!this.bufferReader.AdvanceRead())
                {
                    await this.Disconnect(cancellationToken, WebSocketCloseStatus.InternalServerError);
                    break;
                }
            }
            catch (OperationCanceledException)
            {
            }
        }
    }

    public ValueTask SendAsync(byte[] data, CancellationToken cancellationToken) => this.socket.SendAsync(
        data,
        WebSocketMessageType.Binary,
        WebSocketMessageFlags.EndOfMessage,
        cancellationToken
    );
}