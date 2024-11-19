using DiscordGames.Core;
using DiscordGames.Grains.States;

namespace DiscordGames.Grains.Interfaces.GameSessions;

[Alias("DiscordGames.Grain.GameSessions.IPerudoSessionGrain")]
public interface IPerudoSessionGrain : IGameSessionGrain
{
    [Alias("InitSession")]
    ValueTask<ResultCode> InitSession(UserId userId, string sessionName, int maxPlayer, bool isClassicRule);

    [Alias("EditSession")]
    ValueTask<ResultCode> EditSession(UserId userId, string sessionName, int maxPlayer, bool isClassicRule);

    [Alias("PlaceBid")]
    ValueTask<ResultCode> PlaceBid(int userId, int quantity, int face);

    [Alias("Challenge")]
    ValueTask<ResultCode> Challenge(UserId userId);

    [Alias("GetState")]
    Task<PerudoSessionState> GetState();
}