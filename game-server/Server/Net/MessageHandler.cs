using DiscordGames.Core.Net;
using DiscordGames.Core.Net.Message;
using DiscordGames.Core.Net.Serialize;
using DiscordGames.Grain.Interfaces.GameSessions;
using Microsoft.Extensions.Logging;
using PooledAwait;
using WebSocketSharp;
using WebSocketSharp.NetCore;
using WebSocketSharp.NetCore.Server;
using ErrorEventArgs = WebSocketSharp.NetCore.ErrorEventArgs;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace DiscordGames.Server.Net;

public class MessageHandler : WebSocketBehavior, IMessageHandler
{
    private ILogger<MessageHandler> logger = default!;
    private UserId userId;
    
    protected override void OnOpen()
    {
        this.logger = ServiceLocator.LoggerFactory.CreateLogger<MessageHandler>();
    }

    protected override void OnClose(CloseEventArgs e)
    {
        this.logger.LogTrace("CLOSED : {reason} ({clean})", e.Reason, e.WasClean);
        this.Sessions.CloseSession(this.ID);
    }

    protected override void OnError(ErrorEventArgs e)
    {
        this.logger.LogError("ERROR : {message}", e.Message);
        this.Sessions.CloseSession(this.ID);
    }

    protected override void OnMessage(MessageEventArgs e)
    {
        if (!e.IsBinary)
        {
            this.logger.LogMessageWasNotBinary(e.IsText ? e.Data : "(ping)");
            return;
        }
        
        MessageSerializer.Read(e.RawData, this);
    }
    
    public ValueTask OnGreeting(GreetingMessage message)
    {
        this.userId = -1;
        this.Send(MessageSerializer.WriteGreetingMessage(MessageChannel.Direct, this.userId, message.DiscordUid));
        this.logger.LogOnGreeting(this.ID, message.DiscordUid);
        return ValueTask.CompletedTask;
    }
    
    public ValueTask OnPing(PingMessage message)
    {
        var utcTicks = DateTime.UtcNow.Ticks;
        this.Send(MessageSerializer.WritePingMessage(MessageChannel.Direct, utcTicks));
        this.logger.LogOnPing(this.ID, message.UtcTicks, utcTicks);
        return ValueTask.CompletedTask;
    }

    public ValueTask OnHostGame(HostGameMessage message)
    {
        return Internal(this, message);
        static async PooledValueTask Internal(MessageHandler self, HostGameMessage message)
        {
            switch (message.Data)
            {
                case PerudoHostGameData perudo:
                {
                    var session = ServiceLocator.GrainFactory.GetGrain<IPerudoSessionGrain>(Guid.NewGuid());
                    await session.InitSession(self.userId, message.Name, perudo.MaxPlayers, perudo.IsClassicRule);
                    self.Send(session.GetPrimaryKey().ToString());
                    break;
                }
                default: throw new NotImplementedException();
            }
        }
    }
}

public static partial class Log
{
    [LoggerMessage(LogLevel.Warning,
        Message = "Message has non-binary data [data : {data}]")]
    public static partial void LogMessageWasNotBinary(this ILogger logger, string data);

    [LoggerMessage(LogLevel.Information,
        Message = "On Greeting [Connection UID : {connectionUid}, Discord UID : {discordUid}]")]
    public static partial void LogOnGreeting(this ILogger logger, string connectionUid, string discordUid);

    [LoggerMessage(LogLevel.Trace,
        Message = "On Ping [Connection UID : {connectionUid}, Client UTC Ticks : {clientUtcTicks}, Server UTC Ticks : {serverUtcTicks}]")]
    public static partial void LogOnPing(this ILogger logger, string connectionUid, long clientUtcTicks, long serverUtcTicks);
}