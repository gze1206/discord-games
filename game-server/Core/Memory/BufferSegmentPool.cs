using System;
using System.Threading;

namespace DiscordGames.Core.Memory
{
    public class BufferSegmentPool
    {
        private static readonly Lazy<BufferSegmentPool> Instance = new(LazyThreadSafetyMode.ExecutionAndPublication);
        public static BufferSegmentPool I => Instance.Value;

        public BufferSegmentPool()
        {
            
        }
    }
}

