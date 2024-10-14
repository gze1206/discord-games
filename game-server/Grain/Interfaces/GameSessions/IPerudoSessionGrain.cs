using DiscordGames.Core;

namespace DiscordGames.Grain.Interfaces.GameSessions;

[Alias("Server.Grains.GameSessions.IPerudoSessionGrain")]
public interface IPerudoSessionGrain : IGameSessionGrain
{
    [Alias("PlaceBid")]
    Task<PlaceBidResult> PlaceBid(int userId, int quantity, int face);
}