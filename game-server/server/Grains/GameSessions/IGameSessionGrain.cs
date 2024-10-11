using server.Core;

namespace server.Grains.GameSessions;

[Alias("server.Grains.IGameSessionGrain")]
public interface IGameSessionGrain : IGrainWithGuidKey
{
    [Alias("JoinGame")] Task<JoinGameResult> JoinGame(UserIdType userId);
    [Alias("LeaveGame")] Task<LeaveGameResult> LeaveGame(UserIdType userId);
    [Alias("JoinAsSpector")] Task JoinAsSpector(UserIdType userId);
    [Alias("LeaveSpector")] Task LeaveSpector(UserIdType userId);
    [Alias("StartGame")] Task StartGame();
}