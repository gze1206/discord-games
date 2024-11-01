using System;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace DiscordGames.Core.Memory.Pool;

public class PinnedObjectHeapPool : IMemoryPool
{
    private bool isDisposed;
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
#if NET6_0_OR_GREATER
            : GC.AllocateArray<byte>(MemoryPool.SegmentSize, pinned: true);
#else
            : throw new NotSupportedException("Cannot use POH when dotnet version is less than 6");
#endif
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
