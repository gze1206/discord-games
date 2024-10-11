using server.Core;

namespace server.Grains.GameSessions;

public interface IPerudoSessionGrain : IGameSessionGrain
{
    Task<PlaceBidResult> PlaceBid(int userId, int quantity, int face);
}