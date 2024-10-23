using System;

namespace DiscordGames.Core.Memory.Pool;

public interface IMemoryPool : IDisposable
{
    byte[] Rent();
    void Return(byte[] buffer);
}