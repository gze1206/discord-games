namespace DiscordGames.WebServer.LogMessages;

public static partial class Log
{
    [LoggerMessage(
        LogLevel.Critical,
        message: "Caught exceptions"
    )]
    public static partial void LogCaughtException(this ILogger logger, Exception exception);
}