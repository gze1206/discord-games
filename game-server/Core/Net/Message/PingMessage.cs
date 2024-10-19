namespace DiscordGames.Core.Net.Message
{
    public readonly partial struct PingMessage : IMessage
    {
        public long UtcTicks { get; }
    }
    
    public readonly partial struct PingMessage
    {
        public MessageHeader Header { get; }
        
        public PingMessage(MessageHeader Header, long UtcTicks)
        {
            this.Header = Header;
            this.UtcTicks = UtcTicks;
        }
    }
}

