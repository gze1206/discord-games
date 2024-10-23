using System;
using System.Collections.Concurrent;

namespace DiscordGames.Core.Memory.Pool;

public class SmallObjectHeapPool : IMemoryPool
{
    private ConcurrentQueue<byte[]> pool = new();

    public byte[] Rent()
    {
        return this.pool.TryDequeue(out var array)
            ? array
            : new byte[MemoryPool.SegmentSize];
    }

    public void Return(byte[] buffer)
    {
        this.pool.Enqueue(buffer);
    }

    public void Dispose()
    {
        this.pool.Clear();
        this.pool = null;
        GC.SuppressFinalize(this);
    }
}