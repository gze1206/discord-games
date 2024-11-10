namespace DiscordGames.WebServer.LogMessages.Services;

public static partial class Log
{
    [LoggerMessage(
        LogLevel.Information,
        message: "Closing inactive connection with {address} ({userId}) [inactiveSeconds : {inactiveSeconds}]"
    )]
    public static partial void LogOnClosing(this ILogger logger, string address, UserId userId, double inactiveSeconds);
}