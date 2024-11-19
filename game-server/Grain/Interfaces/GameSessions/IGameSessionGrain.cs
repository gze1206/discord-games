using DiscordGames.Core;

namespace DiscordGames.Grains.Interfaces.GameSessions;

[Alias("DiscordGames.Grain.IGameSessionGrain")]
public interface IGameSessionGrain : IGrainWithStringKey
{
    [Alias("JoinGame")] ValueTask<ResultCode> JoinPlayer(UserId userId);
    [Alias("LeaveGame")] ValueTask<ResultCode> LeavePlayer(UserId userId);
    [Alias("JoinAsSpector")] ValueTask<ResultCode> JoinSpectator(UserId userId);
    [Alias("LeaveSpector")] ValueTask<ResultCode> LeaveSpectator(UserId userId);
    [Alias("LeaveUser")] ValueTask<ResultCode> LeaveUser(UserId userId);
    [Alias("StartGame")] ValueTask<ResultCode> StartGame(UserId userId);
}
