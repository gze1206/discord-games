namespace DiscordGames.Core.Net.Message
{
    public readonly partial record struct GreetingMessage : IMessage
    {
        public int UserId { get; init; }
        public long DiscordUid { get; init; }
    }
}

