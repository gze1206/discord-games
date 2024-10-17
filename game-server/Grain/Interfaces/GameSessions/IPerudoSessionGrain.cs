using DiscordGames.Core;
using DiscordGames.Grain.States;

namespace DiscordGames.Grain.Interfaces.GameSessions;

[Alias("Server.Grains.GameSessions.IPerudoSessionGrain")]
public interface IPerudoSessionGrain : IGameSessionGrain
{
    [Alias("InitSession")]
    Task<InitPerudoSessionResult> InitSession(UserId userId, int maxPlayer, bool isClassicRule);
    
    [Alias("PlaceBid")]
    Task<PlaceBidResult> PlaceBid(int userId, int quantity, int face);

    [Alias("GetState")]
    Task<PerudoSessionState> GetState();
}