namespace DiscordGames.Core.Net.Message;

public readonly partial record struct EditGameMessage
{
    public string Name { get; init; }
    public IHostGameData Data { get; init; }
}