namespace DiscordGames.Core.Memory.Pool;

public static class MemoryPool
{
    public const int SegmentSize = 64;
    public static IMemoryPool I { get; private set; } = default!;

    public static void Init(IMemoryPool instance)
    {
        I = instance;
    }

    public static void Dispose()
    {
        I.Dispose();
    }
}