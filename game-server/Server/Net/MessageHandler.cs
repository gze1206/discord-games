using DiscordGames.Core.Net;
using DiscordGames.Core.Net.Message;
using DiscordGames.Core.Net.Serialize;
using Microsoft.Extensions.Logging;
using WebSocketSharp;
using WebSocketSharp.Server;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace DiscordGames.Server.Net;

public class MessageHandler : WebSocketBehavior, IMessageHandler
{
    private ILogger<MessageHandler> logger = default!;
    
    protected override void OnOpen()
    {
        this.logger = ServiceLocator.LoggerFactory.CreateLogger<MessageHandler>();
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
        this.Send(MessageSerializer.WriteGreetingMessage(MessageChannel.Direct, -1, message.DiscordUid));
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