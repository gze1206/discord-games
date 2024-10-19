namespace DiscordGames.Core.Net;

public static partial class MessageSerializer
{
    private const byte SchemeVersion = 1;
    private const int VersionBits = 4;
    private const byte VersionMask = byte.MaxValue & (byte.MaxValue << VersionBits);

    internal static uint CalcChecksum(ReadOnlySpan<byte> payload)
    {
        var crc = 0xFFFFFFFF;

        foreach (var b in payload)
        {
            crc ^= b;

            for (var i = 0; i < 8; i++)
            {
                if ((crc & 1) == 1) crc = (crc >> 1) ^ 0xEDB88320;
                else crc >>= 1;
            }
        }
        
        return crc ^ 0xFFFFFFFF;
    }
}