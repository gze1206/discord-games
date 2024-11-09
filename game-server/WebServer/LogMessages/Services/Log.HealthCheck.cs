using Orleans.Serialization;

namespace WebServer.LogMessages.Services;

public static partial class Log
{
    [LoggerMessage(
        LogLevel.Information,
        message: "Closing inactive connection with {address} ({userId}) [inactiveSeconds : {inactiveSeconds}]"
    )]
    public static partial void LogOnClosing(this ILogger logger, string address, UserId userId, double inactiveSeconds);

    [LoggerMessage(
        LogLevel.Critical,
        message: "Caught exceptions {exception}"
    )]
    public static partial void LogCaughtException(this ILogger logger, Exception exception);
}