using System.Buffers;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using DiscordGames.Core.Memory;
using DiscordGames.Core.Net;
using DiscordGames.Core.Net.Serialize;
using PooledAwait;
using Uri = System.Uri;

namespace TestClient.Net;

public class WebSocketWrapper : IAsyncDisposable
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
    
    public async ValueTask DisposeAsync()
    {
        if (this.isDisposed) return;
        
        GC.SuppressFinalize(this);
        
        if (this.buffer != null)
        {
            ArrayPool<byte>.Shared.Return(this.buffer);
            this.buffer = null;
        }
        
        this.socket.Dispose();

        this.isDisposed = true;
    }

    public ValueTask Connect(Uri host, CancellationToken cancellationToken)
    {
        return Internal(this, host, cancellationToken);
        static async PooledValueTask Internal(WebSocketWrapper self, Uri host, CancellationToken cancellationToken)
        {
            await self.socket.ConnectAsync(host, cancellationToken);
            self.OnOpen();
        }
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public async PooledValueTask Disconnect(CancellationToken cancellationToken, WebSocketCloseStatus status = WebSocketCloseStatus.NormalClosure)
    {
        if (this.State is WebSocketState.Open or WebSocketState.CloseReceived)
            await this.socket.CloseOutputAsync(status, string.Empty, cancellationToken);
    }

    public ValueTask Loop(CancellationToken cancellationToken)
    {
        return Internal(this, cancellationToken);
        static async PooledValueTask Internal(WebSocketWrapper self, CancellationToken cancellationToken)
        {
            while (self.socket.State is WebSocketState.Open)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    await self.Disconnect(CancellationToken.None);
                    break;
                }
                
                // 먼저 '읽기 영역'을 버퍼의 가장 앞쪽으로 당겨옵니다 ('쓰기 영역' 공간 확보를 위해)
                self.bufferReader.Compact();
                
                var receiveResult = await self.socket.ReceiveAsync(self.bufferReader.WriteSegment, CancellationToken.None);
                
                // 텍스트 데이터는 받지 않고, 바이트로 직렬화된 것만 받습니다
                if (receiveResult.MessageType is WebSocketMessageType.Text) continue;

                // 접속 종료 메시지라면 접속 해제 처리를 합니다
                if (receiveResult.MessageType is WebSocketMessageType.Close)
                {
                    await self.Disconnect(cancellationToken);
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

                await self.bufferReader.ReadAndHandleMessage(self.handler);

                // 버퍼에서 데이터를 읽었으니 다음에 읽기 시작할 위치를 조정합니다 (이걸 실패하면 비정상이니 연결을 끊어버립니다)
                if (!self.bufferReader.AdvanceRead())
                {
                    await self.Disconnect(cancellationToken, WebSocketCloseStatus.InternalServerError);
                    break;
                }
            }
        }
    }

    public ValueTask SendAsync(byte[] data) => this.socket.SendAsync(
        data,
        WebSocketMessageType.Binary,
        WebSocketMessageFlags.EndOfMessage,
        CancellationToken.None);
}