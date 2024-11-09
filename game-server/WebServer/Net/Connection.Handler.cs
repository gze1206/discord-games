using DiscordGames.Core.Net;
using DiscordGames.Core.Net.Message;
using DiscordGames.Core.Net.Serialize;
using DiscordGames.Grains.Interfaces;
using DiscordGames.Grains.Interfaces.GameSessions;
using DiscordGames.Grains.ResultCodes.PerudoSession;
using PooledAwait;
using WebServer.LogMessages.Net;
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
            self.UserId = await auth.VerifyTokenAndGetUserId(message.DiscordAccessToken);
            
            self.logger.LogInformation("GREETING [{userId}, {discordUid}]", self.UserId, message.DiscordAccessToken);

            await self.PreserveSend(
                MessageSerializer.WriteGreetingMessage(MessageChannel.Direct, self.UserId, message.DiscordAccessToken));

            self.sendTask ??= self.ProcessSend();

            if (!ConnectionPool.I.Register(self.UserId, self)) CoreThrowHelper.ThrowInvalidOperation();
        }
    }

    public ValueTask OnPing(PingMessage message)
    {
        var now = DateTime.UtcNow.Ticks;
        var rtt = this.lastPintSentAtUtc == 0
            ? 0
            : (now - this.lastPintSentAtUtc) / TimeSpan.TicksPerMillisecond;
        this.PingMS = (int)(rtt * 0.5);
        this.lastPintSentAtUtc = now;
        
        this.logger.LogOnPing(this.Address, this.UserId, this.PingMS);

        return this.PreserveSend(MessageSerializer.WritePingMessage(MessageChannel.Direct, now));
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