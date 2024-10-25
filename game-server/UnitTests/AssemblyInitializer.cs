using DiscordGames.Core.Memory.Pool;

namespace UnitTests;

[TestClass]
public class AssemblyInitializer
{
    [AssemblyInitialize]
    public static void Init(TestContext context)
    {
        MemoryPool.Init(new PinnedObjectHeapPool());
    }

    [AssemblyCleanup]
    public static void Dispose()
    {
        MemoryPool.Dispose();
    }
}