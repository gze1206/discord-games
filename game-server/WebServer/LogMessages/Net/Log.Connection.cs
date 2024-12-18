namespace DiscordGames.WebServer.LogMessages.Net;

public static partial class Log
{
    [LoggerMessage(
        LogLevel.Information,
        message: "Connected {socket}"
    )]
    public static partial void LogOnConnected(this ILogger logger, string socket);
    
    [LoggerMessage(
        LogLevel.Information,
        message: "Disconnected {socket}"
    )]
    public static partial void LogOnDisconnected(this ILogger logger, string socket);
    
    [LoggerMessage(
        LogLevel.Information,
        message: "Leave user {userId} from session {sessionUid}"
    )]
    public static partial void LogLeaveFromSession(this ILogger logger, UserId userId, string sessionUid);
    
    [LoggerMessage(
        LogLevel.Warning,
        message: "Received Text data from {socket}"
    )]
    public static partial void LogOnTextData(this ILogger logger, string socket);

    [LoggerMessage(
        LogLevel.Information,
        message: "GREETING {userId}"
    )]
    public static partial void LogOnGreeting(this ILogger logger, UserId userId);
    
    [LoggerMessage(
        LogLevel.Trace,
        message: "PING {ping}ms [{address} ({userId})]"
    )]
    public static partial void LogOnPing(this ILogger logger, string address, UserId userId, long ping);
}