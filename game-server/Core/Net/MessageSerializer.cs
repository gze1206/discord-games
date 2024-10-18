using DiscordGames.Core.Net.Message;

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

    public static void Read(ReadOnlySpan<byte> data, IMessageHandler handler)
    {
        var version = (byte)((data[0] & VersionMask) >> VersionBits);
        if (version != SchemeVersion) throw MessageSchemeVersionException.I;
        
        var channel = (MessageChannel)(data[0] & ~VersionMask);
        var type = (MessageType)data[1];
        var checksum = BitConverter.ToUInt32(data[2..6]);

        const int payloadBeginAt = MessageHeader.HeaderSize + 1;
        var actualChecksum = CalcChecksum(data[payloadBeginAt..]);
        if (checksum != actualChecksum) throw InvalidMessageChecksumException.I;
        
        var header = new MessageHeader(version, channel, type)
        {
            Checksum = checksum
        };

        switch (type)
        {
            case MessageType.Ping:
                /* UtcTicks */ var _0 = BitConverter.ToInt64(data[(payloadBeginAt + 0)..(payloadBeginAt + 8)]);
                handler.OnPing(new PingMessage(_0) { Header = header });
                break;
            default: throw new NotImplementedException();
        }
    }

    public static byte[] Write(this PingMessage message)
    {
        var buffer = new Span<byte>(new byte[MessageHeader.HeaderSize + PingMessage.PayloadSize]);

        // Header (Except checksum)
        buffer[0] |= (byte)(VersionMask & (message.Header.SchemeVersion << VersionBits));
        buffer[0] |= (byte)(~VersionMask & (byte)message.Header.Channel);
        buffer[1] = (byte)message.Header.MessageType;
        
        const int payloadBeginAt = MessageHeader.HeaderSize + 1;

        // Payload
        BitConverter.TryWriteBytes(buffer[(payloadBeginAt + 0)..(payloadBeginAt + 8)], message.UtcTicks);

        // Checksum
        BitConverter.TryWriteBytes(buffer[2..6], CalcChecksum(buffer[payloadBeginAt..]));
        
        return buffer.ToArray();
    }
}