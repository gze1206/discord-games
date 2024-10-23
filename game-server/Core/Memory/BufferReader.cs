using System;

namespace DiscordGames.Core.Memory;

public struct BufferReader
{
    private readonly byte[] buffer;

    public int Index { get; private set; }

    public BufferReader(byte[] buffer)
    {
        this.buffer = buffer;
        this.Index = 0;
    }

    internal ReadOnlySpan<byte> Slice(int length)
    {
        var span = this.buffer.AsSpan(this.Index, length);
        this.Index += length;
        return span;
    }
}