using System.Net.WebSockets;
using PooledAwait;

namespace WebServer.Net;

public class Connection
{
    private const int ReadBufferSize = 1024;
    
    private readonly WebSocket socket;
    private readonly Memory<byte> readBuffer;

    public Connection(WebSocket socket)
    {
        this.socket = socket;
        this.readBuffer = GC.AllocateArray<byte>(ReadBufferSize, pinned: true);
    }

    public PooledValueTask Loop(CancellationToken cancellationToken)
    {
        return Internal(this, cancellationToken);
        static async PooledValueTask Internal(Connection self, CancellationToken cancellationToken)
        {
            while (self.socket.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested)
            {
                var receiveResult = await self.socket.ReceiveAsync(self.readBuffer, cancellationToken);
                if (receiveResult.MessageType == WebSocketMessageType.Text) continue;
                if (receiveResult.MessageType == WebSocketMessageType.Close && receiveResult.EndOfMessage) break;
            }
        }
    }
}