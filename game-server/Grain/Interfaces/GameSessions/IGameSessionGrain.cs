using DiscordGames.Grains.ResultCodes.CommonSession;
using Microsoft.Extensions.Logging;

namespace DiscordGames.Grains.Interfaces.GameSessions;

[Alias("Server.Grains.IGameSessionGrain")]
public interface IGameSessionGrain : IGrainWithGuidKey
{
    [Alias("JoinGame")] ValueTask<JoinPlayerResult> JoinPlayer(UserId userId);
    [Alias("LeaveGame")] ValueTask<LeavePlayerResult> LeavePlayer(UserId userId);
    [Alias("JoinAsSpector")] ValueTask<JoinSpectatorResult> JoinSpectator(UserId userId);
    [Alias("LeaveSpector")] ValueTask<LeaveSpectatorResult> LeaveSpectator(UserId userId);
    [Alias("StartGame")] ValueTask<StartGameResult> StartGame(UserId userId);
}

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