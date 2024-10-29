using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using DiscordGames.Core.Memory;
using DiscordGames.Core.Net.Serialize;
using PooledAwait;
using WebServer.LogMessages.Net;

namespace WebServer.Net;

public partial class Connection
{
    private const int ReadBufferSize = 1024;
    
    private readonly WebSocket socket;
    private readonly string address;
    private readonly ILogger<Connection> logger;

    private BufferReader bufferReader;

    public Connection(WebSocket socket, string address, ILogger<Connection> logger)
    {
        this.socket = socket;
        this.address = address;
        this.logger = logger;
        this.bufferReader = new BufferReader(GC.AllocateArray<byte>(ReadBufferSize, pinned: true));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private async PooledValueTask Disconnect(CancellationToken cancellationToken, WebSocketCloseStatus status = WebSocketCloseStatus.NormalClosure)
    {
        await this.socket.CloseOutputAsync(status, string.Empty, cancellationToken);
    }

    public ValueTask Loop(CancellationToken cancellationToken)
    {
        return Internal(this, cancellationToken);
        static async PooledValueTask Internal(Connection self, CancellationToken cancellationToken)
        {
            self.logger.LogOnConnected(self.address);
            
            while (self.socket.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested)
            {
                // 먼저 '읽기 영역'을 버퍼의 가장 앞쪽으로 당겨옵니다 ('쓰기 영역' 공간 확보를 위해)
                self.bufferReader.Compact();
                
                var receiveResult = await self.socket.ReceiveAsync(self.bufferReader.WriteSegment, cancellationToken);
                
                // 텍스트 데이터는 받지 않고, 바이트로 직렬화된 것만 받습니다
                if (receiveResult.MessageType == WebSocketMessageType.Text)
                {
                    self.logger.LogOnTextData(self.address);
                    continue;
                }
                
                // 접속 종료 메시지라면 접속 해제 처리를 합니다
                if (receiveResult.MessageType == WebSocketMessageType.Close)
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
                
                self.bufferReader.ReadAndHandleMessage(self);

                // 버퍼에서 데이터를 읽었으니 다음에 읽기 시작할 위치를 조정합니다 (이걸 실패하면 비정상이니 연결을 끊어버립니다)
                if (!self.bufferReader.AdvanceRead())
                {
                    await self.Disconnect(cancellationToken, WebSocketCloseStatus.InternalServerError);
                    break;
                }
            }
            
            self.logger.LogOnDisconnected(self.address);
        }
    }
}