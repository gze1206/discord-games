namespace WebServer.LogMessages.Net;

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
        LogLevel.Warning,
        message: "Received Text data from {socket}"
    )]
    public static partial void LogOnTextData(this ILogger logger, string socket);
}