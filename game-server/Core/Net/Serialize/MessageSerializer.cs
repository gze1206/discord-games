using System;
using DiscordGames.Core.Memory;
using DiscordGames.Core.Net.Message;

namespace DiscordGames.Core.Net.Serialize;

public static partial class MessageSerializer
{
    // ReSharper disable once MemberCanBePrivate.Global
    internal const byte SchemeVersion = 1;
    
    private const int VersionBits = 4;
    private const byte VersionMask = byte.MaxValue & (byte.MaxValue << VersionBits);
    private const int MessagePrefixSize = sizeof(byte);     // message size (1 byte)
    private const int MessagePostfixSize = sizeof(int);     // CRC Checksum (4 byte)

    // ReSharper disable once MemberCanBePrivate.Global
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
