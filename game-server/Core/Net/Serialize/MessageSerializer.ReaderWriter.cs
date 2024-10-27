using System;
using System.Text;
using DiscordGames.Core.Memory;
using DiscordGames.Core.Net.Message;

namespace DiscordGames.Core.Net.Serialize;

public static partial class MessageSerializer
{
    public static byte ReadByte(this ref BufferReader reader)
    {
        return reader.Slice(1)[0];
    }

    public static bool Write(this ref BufferWriter writer, byte value)
    {
        var span = writer.RequestSpan(1);
        span[0] = value;
        return true;
    }

    public static bool ReadBoolean(this ref BufferReader reader)
    {
        return BitConverter.ToBoolean(reader.Slice(sizeof(bool)));
    }

    public static bool Write(this ref BufferWriter writer, bool value)
    {
        var span = writer.RequestSpan(sizeof(bool));
        return BitConverter.TryWriteBytes(span, value);
    }
    
    public static int ReadInt32(this ref BufferReader reader)
    {
        return BitConverter.ToInt32(reader.Slice(sizeof(int)));
    }

    public static bool Write(this ref BufferWriter writer, int value)
    {
        var span = writer.RequestSpan(sizeof(int));
        return BitConverter.TryWriteBytes(span, value);
    }
    
    public static long ReadInt64(this ref BufferReader reader)
    {
        return BitConverter.ToInt64(reader.Slice(sizeof(long)));
    }

    public static bool Write(this ref BufferWriter writer, long value)
    {
        var span = writer.RequestSpan(sizeof(long));
        return BitConverter.TryWriteBytes(span, value);
    }

    public static string? ReadString(this ref BufferReader reader)
    {
        var length = BitConverter.ToInt32(reader.Slice(sizeof(int)));
        return length < 0
            ? null
            : Encoding.UTF8.GetString(reader.Slice(length));
    }

    public static bool Write(this ref BufferWriter writer, string? value)
    {
        var succeed = true;
        var length = value == null
            ? -1
            : Encoding.UTF8.GetByteCount(value);
        var span = writer.RequestSpan(sizeof(int));
        succeed &= BitConverter.TryWriteBytes(span, length);

        if (length <= 0) return succeed;
        
        span = writer.RequestSpan(length);
        succeed &= (0 < Encoding.UTF8.GetBytes(value.AsSpan(), span) || value!.Length == 0);
        return succeed;
    }

    public static MessageHeader ReadHeader(this ref BufferReader reader)
    {
        var buffer = reader.Slice(MessageHeader.HeaderSize);
        
        var version = (byte)((buffer[0] & VersionMask) >> VersionBits);
        if (version != SchemeVersion) throw MessageSchemeVersionException.I;
        
        var channel = (MessageChannel)(buffer[0] & ~VersionMask);
        var type = (MessageType)buffer[1];

        return new MessageHeader(version, channel, type);
    }

    public static bool Write(this ref BufferWriter writer, MessageHeader header)
    {
        var span = writer.RequestSpan(MessageHeader.HeaderSize);
        span[0] = (byte)(
            VersionMask & (header.SchemeVersion << VersionBits)
            | (~VersionMask & (byte)header.Channel)
        );
        span[1] = (byte)header.MessageType;
        return true;
    }

    public static GameType ReadGameType(this ref BufferReader reader)
    {
        return (GameType)reader.Slice(1)[0];
    }

    public static bool Write(this ref BufferWriter writer, GameType value)
    {
        writer.RequestSpan(1)[0] = (byte)value;
        return true;
    }

    public static IHostGameData ReadIHostGameData(this ref BufferReader reader)
    {
        var type = (GameType)reader.Slice(1)[0];

        switch (type)
        {
            case GameType.Perudo:
            {
                var maxPlayers = BitConverter.ToInt32(reader.Slice(sizeof(int)));
                var isClassicRule = BitConverter.ToBoolean(reader.Slice(sizeof(bool)));
                return new PerudoHostGameData(maxPlayers, isClassicRule);
            }
            default: throw new NotImplementedException();
        }
    }

    public static bool Write(this ref BufferWriter writer, IHostGameData data)
    {
        var succeed = true;
        switch (data)
        {
            case PerudoHostGameData perudo:
            {
                succeed &= writer.Write(GameType.Perudo);
                succeed &= writer.Write(perudo.MaxPlayers);
                succeed &= writer.Write(perudo.IsClassicRule);
                break;
            }
            default: throw new NotImplementedException();
        }

        return succeed;
    }
}
