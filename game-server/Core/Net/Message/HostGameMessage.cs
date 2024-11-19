namespace DiscordGames.Core.Net.Message;

public readonly partial record struct HostGameMessage
{
    public string SessionId { get; init; }
    public string Name { get; init; }
    public IHostGameData Data { get; init; }
}