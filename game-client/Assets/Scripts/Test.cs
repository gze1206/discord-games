using System;
using DiscordGames.Core.Memory;
using DiscordGames.Core.Net.Serialize;
using UnityEngine;

public class Test : MonoBehaviour
{
    private void Start()
    {
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
