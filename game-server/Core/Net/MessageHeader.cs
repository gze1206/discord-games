namespace DiscordGames.Core.Net;

public record struct MessageHeader(
    byte SchemeVersion,         // 4bit
    MessageChannel Channel,     // 4bit
    MessageType MessageType     // 8bit
)
{
    public uint Checksum { get; internal set; }   // 32bit

    internal const int HeaderSize = (4 + 4 + 8 + 32) / 8;
}