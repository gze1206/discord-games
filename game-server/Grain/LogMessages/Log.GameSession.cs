using Microsoft.Extensions.Logging;

// ReSharper disable once CheckNamespace
namespace DiscordGames.Grains.LogMessages.GameSession;

public static partial class Log
{
    [LoggerMessage(LogLevel.Information,
        Message = "Joined {userId} as player to {session}")]
    public static partial void LogJoinPlayerOk(this ILogger logger, Guid session, UserId userId);
    
    [LoggerMessage(LogLevel.Information,
        Message = "Leaved {userId} as player from {session}")]
    public static partial void LogLeavePlayerOk(this ILogger logger, Guid session, UserId userId);
    
    [LoggerMessage(LogLevel.Information,
        Message = "Joined {userId} as spectator to {session}")]
    public static partial void LogJoinSpectatorOk(this ILogger logger, Guid session, UserId userId);
    
    [LoggerMessage(LogLevel.Information,
        Message = "Leaved {userId} as spectator from {session}")]
    public static partial void LogLeaveSpectatorOk(this ILogger logger, Guid session, UserId userId);
    
    [LoggerMessage(LogLevel.Information,
        Message = "Started game by {userId} from {session}")]
    public static partial void LogStartGameOk(this ILogger logger, Guid session, UserId userId);
}
