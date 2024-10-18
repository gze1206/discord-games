namespace DiscordGames.Core.Net;

public record struct MessageHeader(
    byte SchemeVersion,         // 4bit
    MessageChannel Channel,     // 4bit
    MessageType MessageType     // 8bit
)
{
    // uint Checksum            // 32bit - 구조체에는 할당하지 않고, 직렬화 / 역직렬화 처리 중에 바이트 배열 내에서만 존재합니다

    internal const int HeaderSize = (4 + 4 + 8 + 32) / 8;
}