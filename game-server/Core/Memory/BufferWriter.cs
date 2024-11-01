using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using DiscordGames.Core.Memory.Pool;

#pragma warning disable CS8500

namespace DiscordGames.Core.Memory;

public unsafe struct BufferWriter
{
    private bool isDisposed;
    private BufferSegment* head;
    private BufferSegment* tail;
    
    public int UsedTotal { get; private set; }
    
    public BufferWriter()
    {
        this.head = null;
        this.tail = null;
        this.UsedTotal = 0;
        this.isDisposed = false;
    }

    public void Dispose()
    {
        if (this.isDisposed) return;
        
        if (this.head == null)
        {
            this.isDisposed = true;
            return;
        }

        var node = this.head;
        while (node != null)
        {
            var cur = node;
            node = node->Next;
            cur->Dispose();
#if NET6_0_OR_GREATER
            NativeMemory.Free(cur);
#else
            Marshal.FreeHGlobal(new IntPtr(cur));
#endif
        }

        this.head = null;
        this.tail = null;
        this.UsedTotal = 0;
        this.isDisposed = true;
    }

    public Span<byte> RequestSpan(int length)
    {
        if (this.tail == null || MemoryPool.SegmentSize < this.tail->Used + length)
        {
            var size = Unsafe.SizeOf<BufferSegment>();
            var segment =
#if NET6_0_OR_GREATER
                (BufferSegment*)NativeMemory.Alloc((nuint)size);
#else
                (BufferSegment*)Marshal.AllocHGlobal(size).ToPointer();
#endif
            segment->Init();
            if (this.tail != null) this.tail->Next = segment;
            if (this.head == null) this.head = segment;
            this.tail = segment;
        }

        this.UsedTotal += length;
        return this.tail->RequestSpan(length);
    }

    public void CopyTo(byte[] dest, int begin = 0)
    {
        var index = begin;
        var node = this.head;
        while (node != null)
        {
            node->CopyTo(dest, index);
            index += node->Used;
            node = node->Next;
        }
    }
}