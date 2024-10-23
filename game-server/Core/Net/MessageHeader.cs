namespace DiscordGames.Core.Net
{
    public readonly struct MessageHeader
    {
        public byte SchemeVersion { get; }         // 4bit
        public MessageChannel Channel { get; }     // 4bit
        public MessageType MessageType { get; }    // 8bit

        internal const int HeaderSize = (4 + 4 + 8) / 8;

        public MessageHeader(byte schemeVersion, MessageChannel channel, MessageType messageType)
        {
            this.SchemeVersion = schemeVersion;
            this.Channel = channel;
            this.MessageType = messageType;
        }
    }
}

