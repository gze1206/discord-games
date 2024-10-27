namespace DiscordGames.Core.Net.Message;

public readonly partial record struct HostGameMessage
{
    public string Name { get; init; }
    public IHostGameData Data { get; init; }
}

public interface IHostGameData { }

public record PerudoHostGameData(
    int MaxPlayers,
    bool IsClassicRule
) : IHostGameData;