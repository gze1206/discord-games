using DiscordGames.Grains.ResultCodes.CommonSession;

namespace DiscordGames.Grains.Interfaces.GameSessions;

[Alias("DiscordGames.Grain.IGameSessionGrain")]
public interface IGameSessionGrain : IGrainWithGuidKey
{
    [Alias("JoinGame")] ValueTask<JoinPlayerResult> JoinPlayer(UserId userId);
    [Alias("LeaveGame")] ValueTask<LeavePlayerResult> LeavePlayer(UserId userId);
    [Alias("JoinAsSpector")] ValueTask<JoinSpectatorResult> JoinSpectator(UserId userId);
    [Alias("LeaveSpector")] ValueTask<LeaveSpectatorResult> LeaveSpectator(UserId userId);
    [Alias("StartGame")] ValueTask<StartGameResult> StartGame(UserId userId);
}
