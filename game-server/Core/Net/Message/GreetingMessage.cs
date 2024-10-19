namespace DiscordGames.Core.Net.Message;

public readonly partial record struct GreetingMessage(
    UserId UserId,
    long DiscordUid
) : IMessage;