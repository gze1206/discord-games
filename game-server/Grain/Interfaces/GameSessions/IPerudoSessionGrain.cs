using DiscordGames.Grain.ResultCodes.PerudoSession;
using DiscordGames.Grain.States;

namespace DiscordGames.Grain.Interfaces.GameSessions;

[Alias("Server.Grains.GameSessions.IPerudoSessionGrain")]
public interface IPerudoSessionGrain : IGameSessionGrain
{
    [Alias("InitSession")]
    ValueTask<InitPerudoSessionResult> InitSession(UserId userId, int maxPlayer, bool isClassicRule);

    [Alias("PlaceBid")]
    ValueTask<PlaceBidResult> PlaceBid(int userId, int quantity, int face);

    [Alias("Challenge")]
    ValueTask<ChallengeResult> Challenge(UserId userId);

    [Alias("GetState")]
    ValueTask<PerudoSessionState> GetState();
}