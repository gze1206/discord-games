namespace DiscordGames.Core;

#region GameSession Grain Results

public enum InitSessionResult
{
    Ok,
    
    AlreadyInitialized = -1,
    AlreadyStarted = -2,
}

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

public enum PlaceBidResult
{
    Ok,
    
    CannotLowerQuantityBid,
    CannotLowerFaceBid,
    InvalidFace,
}
