namespace DiscordGames.Core.Net;

public interface IHostGameData { }

public record PerudoHostGameData(
    int MaxPlayers,
    bool IsClassicRule
) : IHostGameData;