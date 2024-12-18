using Microsoft.Extensions.Logging;

// ReSharper disable once CheckNamespace
namespace DiscordGames.Grains.LogMessages.Perudo;

public static partial class Log
{
    [LoggerMessage(LogLevel.Information,
        Message = "Session {sessionName}({session}) initialized by {userId} [maxPlayer : {maxPlayer}, isClassicRule : {isClassicRule}]")]
    public static partial void LogPerudoInitSessionOk(this ILogger logger, string session, UserId userId, string sessionName, int maxPlayer, bool isClassicRule);
    
    [LoggerMessage(LogLevel.Information,
        Message = "Session {sessionName}({session}) modified by {userId} [maxPlayer : {maxPlayer}, isClassicRule : {isClassicRule}]")]
    public static partial void LogPerudoEditSessionOk(this ILogger logger, string session, UserId userId, string sessionName, int maxPlayer, bool isClassicRule);
    
    [LoggerMessage(LogLevel.Information,
        Message = "New round started from {session} [firstPlayer : {firstUserId}]")]
    public static partial void LogPerudoStartRound(this ILogger logger, string session, UserId firstUserId);
    
    [LoggerMessage(LogLevel.Information,
        Message = "Player {userId} placed bid from {session} [lastBid : ({lastQuantity}, {lastFace}), newBid : ({quantity}, {face})]")]
    public static partial void LogPerudoPlaceBidOk(this ILogger logger, string session, UserId userId, int lastQuantity, int lastFace, int quantity, int face);
    
    [LoggerMessage(LogLevel.Information,
        Message = "Player {userId} challenged to {bidder} from {session} [lastBid : ({lastQuantity}, {lastFace}), actualQuantity : {actualQuantity}]")]
    public static partial void LogPerudoChallengeOk(this ILogger logger, string session, UserId userId, UserId bidder, int lastQuantity, int lastFace, int actualQuantity);
}