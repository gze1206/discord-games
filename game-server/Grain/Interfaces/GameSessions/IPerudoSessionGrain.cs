using DiscordGames.Grains.ResultCodes.PerudoSession;
using DiscordGames.Grains.States;

namespace DiscordGames.Grains.Interfaces.GameSessions;

[Alias("DiscordGames.Grain.GameSessions.IPerudoSessionGrain")]
public interface IPerudoSessionGrain : IGameSessionGrain
{
    [Alias("InitSession")]
    ValueTask<InitPerudoSessionResult> InitSession(UserId userId, string sessionName, int maxPlayer, bool isClassicRule);

    [Alias("PlaceBid")]
    ValueTask<PlaceBidResult> PlaceBid(int userId, int quantity, int face);

    [Alias("Challenge")]
    ValueTask<ChallengeResult> Challenge(UserId userId);

    [Alias("GetState")]
    Task<PerudoSessionState> GetState();
}