using System.Net.WebSockets;
using DiscordGames.Core.Net;
using DiscordGames.Core.Net.Message;
using DiscordGames.Core.Net.Serialize;
using DiscordGames.Grains.Interfaces;
using DiscordGames.Grains.Interfaces.GameSessions;
using DiscordGames.Grains.ResultCodes.PerudoSession;
using PooledAwait;

using static DiscordGames.Grains.Constants;

namespace WebServer.Net;

public partial class Connection : IMessageHandler
{
    public ValueTask OnGreeting(GreetingMessage message)
    {
        return Internal(this, message);
        static async PooledValueTask Internal(Connection self, GreetingMessage message)
        {
            var auth = self.cluster.GetGrain<IAuthGrain>(SingletonGrainId);
            var userId = await auth.VerifyTokenAndGetUserId(message.DiscordAccessToken);
            
            self.logger.LogInformation("GREETING [{userId}, {discordUid}]", userId, message.DiscordAccessToken);
            
            await self.socket.SendAsync(
                MessageSerializer.WriteGreetingMessage(MessageChannel.Direct, userId, message.DiscordAccessToken),
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