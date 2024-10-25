#if USE_BUFFER_MEMORY
using System;
using System.Diagnostics;
using System.Threading;

namespace DiscordGames.Core.Memory.Pool;

public class BufferMemory
{
    private readonly string guid;
    public byte[] Buffer { get; }

    private SpinLock spinLock;
    private int refCnt;

    public BufferMemory(byte[] buffer)
    {
        this.guid = Guid.NewGuid().ToString();
        this.Buffer = buffer;
    }

    internal int GetRefCnt()
    {
        var lockTaken = false;
        try
        {
            this.spinLock.Enter(ref lockTaken);
            return this.refCnt;
        }
        finally
        {
            if (lockTaken) this.spinLock.Exit();
        }
    }

    public void AddRef()
    {
        var lockTaken = false;
        try
        {
            this.spinLock.Enter(ref lockTaken);
            this.refCnt++;
            Console.WriteLine($"REF CNT {this.refCnt} - {this.guid}");
        }
        finally
        {
            if (lockTaken) this.spinLock.Exit();
        }
    }

    public void Release()
    {
        var lockTaken = false;
        try
        {
            this.spinLock.Enter(ref lockTaken);
            this.refCnt--;
            Debug.Assert(0 <= this.refCnt, "참조 카운터는 음수가 될 수 없습니다");
            Console.WriteLine($"REF CNT {this.refCnt} - {this.guid}");
        }
        finally
        {
            if (lockTaken) this.spinLock.Exit();
        }
    }
}
#endif