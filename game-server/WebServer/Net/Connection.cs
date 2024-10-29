using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using DiscordGames.Core.Memory;
using DiscordGames.Core.Net.Serialize;
using PooledAwait;

namespace WebServer.Net;

public partial class Connection
{
    private const int ReadBufferSize = 1024;
    
    private readonly WebSocket socket;
    
    private BufferReader readBuffer;

    public Connection(WebSocket socket)
    {
        this.socket = socket;
        this.readBuffer = new BufferReader(GC.AllocateArray<byte>(ReadBufferSize, pinned: true));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private async PooledValueTask Disconnect(CancellationToken cancellationToken, WebSocketCloseStatus status = WebSocketCloseStatus.NormalClosure)
    {
        await this.socket.CloseOutputAsync(status, string.Empty, cancellationToken);
    }

    public PooledValueTask Loop(CancellationToken cancellationToken)
    {
        return Internal(this, cancellationToken);
        static async PooledValueTask Internal(Connection self, CancellationToken cancellationToken)
        {
            while (self.socket.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested)
            {
                self.readBuffer.Compact();
                var receiveResult = await self.socket.ReceiveAsync(self.readBuffer.WriteSegment, cancellationToken);
                // if (receiveResult.MessageType == WebSocketMessageType.Text) continue;
                if (receiveResult.MessageType == WebSocketMessageType.Close)
                {
                    await self.Disconnect(cancellationToken);
                    break;
                }
                if (receiveResult.Count <= 0) continue;

                if (!self.readBuffer.AdvanceWrite(receiveResult.Count))
                {
                    await self.Disconnect(cancellationToken, WebSocketCloseStatus.InternalServerError);
                    break;
                }
                
                // MessageSerializer.Read(null, self);
                await self.socket.SendAsync(self.readBuffer.Slice(receiveResult.Count).ToArray(), WebSocketMessageType.Text,
                    WebSocketMessageFlags.DisableCompression | WebSocketMessageFlags.EndOfMessage, cancellationToken);

                if (!self.readBuffer.AdvanceRead())
                {
                    await self.Disconnect(cancellationToken, WebSocketCloseStatus.InternalServerError);
                    break;
                }
            }
        }
    }
}