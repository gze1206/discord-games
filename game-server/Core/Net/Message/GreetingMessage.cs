namespace DiscordGames.Core.Net.Message;

public readonly partial record struct GreetingMessage
{
    public int UserId { get; init; }
    public string DiscordAccessToken { get; init; }
}