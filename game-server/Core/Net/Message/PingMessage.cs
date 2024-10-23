namespace DiscordGames.Core.Net.Message
{
    public readonly partial record struct PingMessage
    {
        public long UtcTicks { get; }
    }
    
    public readonly partial record struct PingMessage
    {
        public MessageHeader Header { get; }
        
        public PingMessage(ref MessageHeader Header, long UtcTicks)
        {
            this.Header = Header;
            this.UtcTicks = UtcTicks;
        }
    }
}

