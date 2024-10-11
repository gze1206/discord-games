using server.Core;

namespace server.Grains.GameSessions;

public class PerudoSessionGrain : Grain, IPerudoSessionGrain
{
    private readonly HashSet<UserIdType> players = new();
    
    public Task<JoinGameResult> JoinGame(UserIdType userId)
    {
        return Task.FromResult(this.players.Add(userId)
            ? JoinGameResult.Ok
            : JoinGameResult.AlreadyJoined);
    }

    public Task<LeaveGameResult> LeaveGame(UserIdType userId)
    {
        return Task.FromResult(this.players.Remove(userId)
            ? LeaveGameResult.Ok
            : LeaveGameResult.NotJoinedUser);
    }

    public Task JoinAsSpector(UserIdType userId)
    {
        throw new NotImplementedException();
    }

    public Task LeaveSpector(UserIdType userId)
    {
        throw new NotImplementedException();
    }

    public Task StartGame()
    {
        throw new NotImplementedException();
    }

    public Task<PlaceBidResult> PlaceBid(UserIdType userId, int quantity, int face)
    {
        throw new NotImplementedException();
    }
}