using DiscordGames.Core.Memory.Pool;
using DiscordGames.Server.Serialization.Json;

// ReSharper disable once CheckNamespace
namespace UnitTests.Utils;

public static class Globals
{
    private static int isInitialized = -1;
    
    public static CustomJsonGrainStorageSerializer Serializer()
        => new(
            new CustomJsonConvertBuilder()
                .Add<LinkedListJsonConverter<int>>()
                .BakeOptions()
        );

    public static void Init()
    {
        if (Interlocked.CompareExchange(ref isInitialized, 1, -1) != -1) return;
        MemoryPool.Init(new PinnedObjectHeapPool());
    }
}