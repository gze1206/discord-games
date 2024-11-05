// ReSharper disable once CheckNamespace
namespace DiscordGames.Grains.ResultCodes.PerudoSession;

public enum InitPerudoSessionResult
{
    Ok,
        
    AlreadyInitialized = -1,
    AlreadyStarted = -2,
    InvalidMaxPlayer = -3,
}

public enum PlaceBidResult
{
    Ok,
        
    NotStartedGame = -1,
    NotFromCurrentTurnUser = -2,
    CannotLowerQuantityBid = -3,
    CannotLowerFaceBid = -4,
    InvalidQuantity = -5,
    InvalidFace = -6,
}

public enum ChallengeResult
{
    Ok,
        
    NotStartedGame = -1,
    NotFromCurrentTurnUser = -2,
    NoPreviousBid = -3,
}