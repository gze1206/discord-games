using System;

namespace DiscordGames.Core.Memory.Pool;

public interface IMemoryPool : IDisposable
{
#if USE_BUFFER_MEMORY
    BufferMemory Rent();
    void Return(BufferMemory buffer);
#else
    byte[] Rent();
    void Return(byte[] buffer);
#endif
}
