using System;
using System.Collections.Concurrent;

namespace DiscordGames.Core.Memory.Pool;

public class PinnedObjectHeapPool : IMemoryPool
{
    private ConcurrentQueue<byte[]> pool = new();

    public byte[] Rent()
    {
        return this.pool.TryDequeue(out var array)
            ? array
            : GC.AllocateArray<byte>(MemoryPool.SegmentSize, pinned: true);
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