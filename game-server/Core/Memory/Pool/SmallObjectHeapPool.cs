using System;
using System.Collections.Concurrent;

namespace DiscordGames.Core.Memory.Pool;

public class SmallObjectHeapPool : IMemoryPool
{
    private bool isDisposed;
#if USE_BUFFER_MEMORY
    private readonly ConcurrentQueue<BufferMemory> pool = new();

    public BufferMemory Rent()
    {
        return this.pool.TryDequeue(out var array)
            ? array
            : new(new byte[MemoryPool.SegmentSize]);
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
            : new byte[MemoryPool.SegmentSize];
    }

    public void Return(byte[] buffer)
    {
        this.pool.Enqueue(buffer);
    }
#endif

    public void Dispose()
    {
        if (this.isDisposed) return;
        
        this.pool.Clear();
        this.isDisposed = true;
        GC.SuppressFinalize(this);
    }
}