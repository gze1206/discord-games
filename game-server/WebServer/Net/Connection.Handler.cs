using System.Net.WebSockets;
using DiscordGames.Core.Net;
using DiscordGames.Core.Net.Message;
using DiscordGames.Core.Net.Serialize;
using PooledAwait;

namespace WebServer.Net;

public partial class Connection : IMessageHandler
{
    public ValueTask OnGreeting(GreetingMessage message)
    {
        return Internal(this, message);
        static async PooledValueTask Internal(Connection self, GreetingMessage message)
        {
            self.logger.LogInformation("GREETING [{discordUid}]", message.DiscordUid);
            
            await self.socket.SendAsync(
                MessageSerializer.WriteGreetingMessage(MessageChannel.Direct, -1, message.DiscordUid),
                WebSocketMessageType.Binary,
                WebSocketMessageFlags.EndOfMessage,
                CancellationToken.None
            );
        }
    }

    public ValueTask OnPing(PingMessage message)
    {
        this.logger.LogInformation("PING [{ticks}]", message.UtcTicks);
        
        return ValueTask.CompletedTask;
    }

    public ValueTask OnHostGame(HostGameMessage message)
    {
        this.logger.LogInformation("HOST [{name}]", message.Name);
        
        return ValueTask.CompletedTask;
    }
}