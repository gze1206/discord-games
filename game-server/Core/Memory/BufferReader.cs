using System;

namespace DiscordGames.Core.Memory;

public struct BufferReader
{
    private readonly ArraySegment<byte> buffer;

    private int readSegmentFrom;
    private int writeSegmentFrom;
    private int readOffset;

    public int DataSize => this.writeSegmentFrom - this.readSegmentFrom;
    public int FreeSize => this.buffer.Count - this.writeSegmentFrom;

    public ArraySegment<byte> WriteSegment =>
        new(this.buffer.Array!, this.buffer.Offset + this.writeSegmentFrom, this.FreeSize);
    public ReadOnlySpan<byte> ReadSegment =>
        new(this.buffer.Array!, this.readSegmentFrom, this.DataSize);

    public BufferReader(ArraySegment<byte> buffer)
    {
        this.buffer = buffer;
        this.readSegmentFrom = 0;
        this.writeSegmentFrom = 0;
        this.readOffset = 0;
    }

    public void Compact()
    {
        var dataSize = this.DataSize;

        if (dataSize == 0)
        {
            this.readSegmentFrom = this.writeSegmentFrom = 0;
        }
        else
        {
            Array.Copy(this.buffer.Array!, this.buffer.Offset + this.readSegmentFrom,
                this.buffer.Array!, this.buffer.Offset, dataSize);

            this.readSegmentFrom = 0;
            this.writeSegmentFrom = dataSize;
        }
    }

    public void AdvanceReadOffset(int length)
    {
        var readFrom = this.readSegmentFrom + this.readOffset;
        if (this.writeSegmentFrom < readFrom + length) CoreThrowHelper.ThrowReadBufferOutOfRange();
        
        this.readOffset += length;
    }

    public bool AdvanceRead()
    {
        if (this.writeSegmentFrom < this.readSegmentFrom + this.readOffset) return false;

        this.readSegmentFrom += this.readOffset;
        this.readOffset = 0;
        return true;
    }

    public bool AdvanceWrite(int length)
    {
        if (this.FreeSize < length) return false;

        this.writeSegmentFrom += length;
        return true;
    }

    public ReadOnlySpan<byte> Slice(int length)
    {
        var readFrom = this.readSegmentFrom + this.readOffset;
        if (this.writeSegmentFrom < readFrom + length) CoreThrowHelper.ThrowReadBufferOutOfRange();
        
        var span = this.buffer.AsSpan(readFrom, length);
        this.readOffset += length;
        return span;
    }
}