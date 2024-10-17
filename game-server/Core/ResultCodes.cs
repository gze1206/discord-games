﻿namespace DiscordGames.Core;

#region GameSession Grain Results

public enum JoinPlayerResult
{
    Ok,
    
    AlreadyJoined = -1,
    AlreadyStarted = -2,
    MaxPlayer = -3,
}

public enum LeavePlayerResult
{
    Ok,
    
    NotJoinedUser = -1,
}

public enum JoinSpectatorResult
{
    Ok,
    
    AlreadyJoined = -1,
}

public enum LeaveSpectatorResult
{
    Ok,
    
    NotJoinedUser = -1,
}

public enum StartGameResult
{
    Ok,
    
    AlreadyStarted = -1,
    NotFromHostUser = -2,
    MinPlayer = -3,
}

#endregion // GameSession Grain Results

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
