namespace DiscordGames.Core.Net.Message
{
    public readonly partial struct GreetingMessage : IMessage
    {
        public int UserId { get; }
        public long DiscordUid { get; }
    }

    public readonly partial struct GreetingMessage
    {
        public MessageHeader Header { get; }
        
        public GreetingMessage(MessageHeader Header, int UserId, long DiscordUid)
        {
            this.Header = Header;
            this.UserId = UserId;
            this.DiscordUid = DiscordUid;
        }
    }
}

