using DiscordGames.Core.Net;
using DiscordGames.Core.Net.Message;
using DiscordGames.Core.Net.Serialize;
using DiscordGames.Grains.Interfaces;
using DiscordGames.Grains.Interfaces.GameSessions;
using DiscordGames.Grains.ResultCodes.PerudoSession;
using DiscordGames.WebServer.LogMessages.Net;
using PooledAwait;
using static DiscordGames.Grains.Constants;

namespace DiscordGames.WebServer.Net;

public partial class Connection : IMessageHandler
{
    public ValueTask OnGreeting(GreetingMessage message)
    {
        return Internal(this, message);
        static async PooledValueTask Internal(Connection self, GreetingMessage message)
        {
            var auth = self.cluster.GetGrain<IAuthGrain>(SingletonGrainId);
            self.UserId = await auth.VerifyTokenAndGetUserId(message.DiscordAccessToken);

            var user = self.cluster.GetGrain<IUserGrain>(self.UserId);
            await user.SetConnect(true);
            
            if (!ConnectionPool.I.Register(self)) WebServerThrowHelper.ThrowFailedToRegisterConnection();
            
            self.logger.LogOnGreeting(self.UserId);

            await user.ReserveSend(
                MessageSerializer.WriteGreetingMessage(MessageChannel.Direct, self.UserId, message.DiscordAccessToken));
        }
    }

    public ValueTask OnPing(PingMessage message)
    {
        var now = DateTime.UtcNow.Ticks;
        var rtt = this.lastPingSentAtUtc == 0
            ? 0
            : (now - this.lastPingSentAtUtc) / TimeSpan.TicksPerMillisecond;
        this.PingMs = (int)(rtt * 0.5);
        this.lastPingSentAtUtc = now;
        
        this.logger.LogOnPing(this.Address, this.UserId, this.PingMs);

        return this.ReserveSend(MessageSerializer.WritePingMessage(MessageChannel.Direct, now));
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
                        self.UserId,
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