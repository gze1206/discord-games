using System;
using DiscordGames.Core.Memory.Pool;

#pragma warning disable CS8500

namespace DiscordGames.Core.Memory;

public unsafe struct BufferSegment
{
#if USE_BUFFER_MEMORY
    private BufferMemory? memory;
#else
    private byte[]? memory;
#endif
    private bool isDisposed;

    public BufferSegment* Next { get; set; }
    public int Used { get; private set; }

    public void Init()
    {
        this.memory = MemoryPool.I.Rent();
#if USE_BUFFER_MEMORY
        this.memory.AddRef();
#endif
        this.Next = null;
        this.Used = 0;
    }

    public void Dispose()
    {
        if (this.isDisposed) return;
        
#if USE_BUFFER_MEMORY
        this.memory?.Release();
#endif
        MemoryPool.I.Return(this.memory!);
        this.memory = null;
        this.Next = null;
        this.Used = 0;
        this.isDisposed = true;
    }

    public Span<byte> RequestSpan(int length)
    {
#if USE_BUFFER_MEMORY
        var span = new Span<byte>(this.memory!.Buffer, this.Used, length);
#else
        var span = new Span<byte>(this.memory, this.Used, length);
#endif
        this.Used += length;
        return span;
    }

    public void CopyTo(byte[] dest, int index)
    {
#if USE_BUFFER_MEMORY
        this.memory!.Buffer.AsSpan(0, this.Used).CopyTo(dest.AsSpan(index, this.Used));
#else
        this.memory!.AsSpan(0, this.Used).CopyTo(dest.AsSpan(index, this.Used));
#endif
    }
}