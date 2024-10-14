using DiscordGames.Core;

namespace DiscordGames.Grain.Interfaces.GameSessions;

[Alias("Server.Grains.IGameSessionGrain")]
public interface IGameSessionGrain : IGrainWithGuidKey
{
    [Alias("JoinGame")] Task<JoinPlayerResult> JoinPlayer(UserId userId);
    [Alias("LeaveGame")] Task<LeavePlayerResult> LeavePlayer(UserId userId);
    [Alias("JoinAsSpector")] Task<JoinSpectatorResult> JoinSpectator(UserId userId);
    [Alias("LeaveSpector")] Task<LeaveSpectatorResult> LeaveSpectator(UserId userId);
    [Alias("StartGame")] Task<StartGameResult> StartGame(UserId userId);
}