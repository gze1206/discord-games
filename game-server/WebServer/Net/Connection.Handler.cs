using System.Net.WebSockets;
using DiscordGames.Core.Net;
using DiscordGames.Core.Net.Message;
using DiscordGames.Core.Net.Serialize;
using DiscordGames.Grain.Interfaces.GameSessions;
using DiscordGames.Grain.ResultCodes.PerudoSession;
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
        return Internal(this, message);
        static async PooledValueTask Internal(Connection self, HostGameMessage message)
        {
            using var _ = self.logger.BeginScope("HOST GAME");
            switch (message.Data)
            {
                case PerudoHostGameData perudo:
                {
                    var session = self.cluster.GetGrain<IPerudoSessionGrain>(Guid.NewGuid());
                    var result = await session.InitSession(
                        -1,
                        message.Name,
                        perudo.MaxPlayers,
                        perudo.IsClassicRule
                    );
                    
                    if (result != InitPerudoSessionResult.Ok) self.logger.LogWarning("RESULT NOT OK [{result}]", result);
                    self.logger.LogInformation("SESSION ID : {id}", session.GetPrimaryKey());
                    break;
                }
                default:
                    CoreThrowHelper.ThrowInvalidOperation();
                    break;
            }
            self.logger.LogInformation("HOST [{name}]", message.Name);
        }
    }
}