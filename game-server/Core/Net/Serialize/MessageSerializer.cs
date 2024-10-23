using System;
using System.Threading.Tasks;
using DiscordGames.Core.Memory;
using DiscordGames.Core.Net.Message;

namespace DiscordGames.Core.Net.Serialize;

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

    public static PingMessage ReadPingMessage(this ref BufferReader reader, ref MessageHeader header)
    {
        var _0 = reader.ReadInt64();
        return new PingMessage(ref header, _0);
    }

    public static bool Write(this ref BufferWriter writer, PingMessage message)
    {
        var succeed = true;
        succeed &= writer.Write(message.Header);
        succeed &= writer.Write(message.UtcTicks);
        return succeed;
    }
        
    public static void Read(byte[] buffer, IMessageHandler handler)
    {
        var checksum = BitConverter.ToInt32(buffer.AsSpan(buffer.Length - sizeof(int)));
        var actualChecksum = CalcChecksum(buffer.AsSpan(0, buffer.Length - sizeof(int)));
        if (checksum != actualChecksum) throw InvalidMessageChecksumException.I;
        
        var reader = new BufferReader(buffer);
        var header = reader.ReadHeader();

        switch (header.MessageType)
        {
            case MessageType.Ping:
            {
                handler.OnPing(reader.ReadPingMessage(ref header));
                break;
            }
            default: throw new NotImplementedException();
        }
    }

    public static byte[] Write(this PingMessage message)
    {
        var writer = new BufferWriter();
        writer.Write(message);
        
        var size = writer.UsedTotal;
        var buffer = new byte[size + sizeof(int)];
        writer.CopyTo(buffer);
        writer.Dispose();
        
        var checksum = CalcChecksum(buffer.AsSpan(0, size));
        BitConverter.TryWriteBytes(buffer.AsSpan(size), checksum);
        return buffer;
    }
}
