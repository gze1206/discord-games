using System;
using DiscordGames.Core.Memory;
using DiscordGames.Core.Memory.Pool;
using DiscordGames.Core.Net.Serialize;
using UnityEngine;

public class Test : MonoBehaviour
{
    private void Start()
    {
        MemoryPool.Init(new SmallObjectHeapPool());
        
        var serialized =
            MessageSerializer.WritePingMessage(
                DiscordGames.Core.Net.MessageChannel.Direct, DateTime.UtcNow.Ticks);

        var str = string.Join(", ", serialized);
        Debug.Log($"<color=yellow>{str}</color>");
        
        var reader = new BufferReader(serialized);
        var header = reader.ReadHeader();
        var message = reader.ReadPingMessage(ref header);
        
        Debug.Log(new DateTime(message.UtcTicks, DateTimeKind.Utc));
    }
}
