using System;
using DiscordGames.Core.Memory.Pool;

#pragma warning disable CS8500

namespace DiscordGames.Core.Memory;

public unsafe struct BufferSegment
{
    private byte[] array;

    public BufferSegment* Next { get; set; }
    public int Used { get; private set; }

    public void Init()
    {
        this.array = MemoryPool.I.Rent() ?? throw new InvalidOperationException("MemoryPool에서 바이트 배열을 얻지 못했습니다.");
        this.Next = null;
        this.Used = 0;
    }

    public void Dispose()
    {
        MemoryPool.I.Return(this.array);
        this.array = null;
    }

    public Span<byte> RequestSpan(int length)
    {
        var span = new Span<byte>(this.array, this.Used, length);
        this.Used += length;
        return span;
    }

    public void CopyTo(Span<byte> dest, int index)
    {
        this.array.AsSpan(0, this.Used).CopyTo(dest[index..(index + this.Used)]);
    }
}