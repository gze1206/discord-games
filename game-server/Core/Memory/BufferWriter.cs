using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using DiscordGames.Core.Memory.Pool;

#pragma warning disable CS8500

namespace DiscordGames.Core.Memory;

public unsafe struct BufferWriter
{
    private BufferSegment* head;
    private BufferSegment* tail;
    
    public int UsedTotal { get; private set; }
    
    public BufferWriter()
    {
        this.head = null;
        this.tail = null;
        this.UsedTotal = 0;
    }

    public void Dispose()
    {
        if (this.head == null) return;

        var node = this.head;
        while (node != null)
        {
            var cur = node;
            node = node->Next;
            cur->Dispose();
            NativeMemory.Free(cur);
        }

        this.head = null;
        this.tail = null;
        this.UsedTotal = 0;
    }

    public Span<byte> RequestSpan(int length)
    {
        if (this.tail == null || MemoryPool.SegmentSize < this.tail->Used + length)
        {
            var size = Unsafe.SizeOf<BufferSegment>();
            var segment = (BufferSegment*)NativeMemory.Alloc((nuint)size);
            segment->Init();
            if (this.tail != null) this.tail->Next = segment;
            if (this.head == null) this.head = segment;
            this.tail = segment;
        }

        this.UsedTotal += length;
        return this.tail->RequestSpan(length);
    }

    public void CopyTo(Span<byte> dest)
    {
        var index = 0;
        var node = this.head;
        while (node != null)
        {
            node->CopyTo(dest, index);
            index += node->Used;
            node = node->Next;
        }
    }
}