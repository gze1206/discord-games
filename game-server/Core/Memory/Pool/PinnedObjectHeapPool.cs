using System;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace DiscordGames.Core.Memory.Pool;

public class PinnedObjectHeapPool : IMemoryPool
{
#if USE_BUFFER_MEMORY
    private readonly ConcurrentQueue<BufferMemory> pool = new();

    public BufferMemory Rent()
    {
        return this.pool.TryDequeue(out var array)
            ? array
            : new(GC.AllocateArray<byte>(MemoryPool.SegmentSize, pinned: true));
    }

    public void Return(BufferMemory buffer)
    {
        Debug.Assert(buffer.GetRefCnt() == 0, $"Memory RefCnt error - returned memory's reference count is {buffer.GetRefCnt()}");
        this.pool.Enqueue(buffer);
    }
#else
    private readonly ConcurrentQueue<byte[]> pool = new();

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
#endif

    public void Dispose()
    {
        this.pool.Clear();
        GC.SuppressFinalize(this);
    }
}