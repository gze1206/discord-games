using server.Core;

namespace server.Grains.GameSessions;

[Alias("server.Grains.GameSessions.IPerudoSessionGrain")]
public interface IPerudoSessionGrain : IGameSessionGrain
{
    [Alias("PlaceBid")]
    Task<PlaceBidResult> PlaceBid(int userId, int quantity, int face);
}