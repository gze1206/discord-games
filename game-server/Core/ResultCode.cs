// ReSharper disable InconsistentNaming
namespace DiscordGames.Core;

public enum ResultCode : uint
{
    Ok,
    Unknown = 9999,
    
    // Game Sessions (1xxx)
    // - Common (10xx)
    __GameSessions_Common = 1000,
    AlreadyJoined,
    AlreadyStarted,
    AlreadyInitialized,
    NotJoinedUser,
    NotInitializedGame,
    NotStartedGame,
    SessionNotFound,
    ExceedMinPlayerLimit,
    ExceedMaxPlayerLimit,
    NotFromHostUser,
    InvalidMinPlayer,
    InvalidMaxPlayer,
    // - Perudo (11xx)
    __GameSessions_Perudo = 1100,
    NotFromCurrentTurnUser,
    CannotLowerQuantityBid,
    CannotLowerFaceBid,
    InvalidQuantity,
    InvalidFace,
    NoPreviousBid,
}