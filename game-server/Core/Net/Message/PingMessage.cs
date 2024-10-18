namespace DiscordGames.Core.Net.Message;

public readonly partial record struct PingMessage(
    long UtcTicks
) : IMessage;
