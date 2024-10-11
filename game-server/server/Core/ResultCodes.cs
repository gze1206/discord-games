namespace server.Core;

public enum JoinGameResult
{
    Ok,
    
    AlreadyJoined = -1,
}

public enum LeaveGameResult
{
    Ok,
    
    NotJoinedUser = -1,
}

public enum PlaceBidResult
{
    Ok,
    
    CannotLowerQuantityBid,
    CannotLowerFaceBid,
    InvalidFace,
}
