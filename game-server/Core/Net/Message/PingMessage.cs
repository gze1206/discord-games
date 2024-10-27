namespace DiscordGames.Core.Net.Message;

public readonly partial record struct PingMessage
{
    public long UtcTicks { get; init; }
}