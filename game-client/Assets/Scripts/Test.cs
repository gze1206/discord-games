using System;
using System.Globalization;
using System.Text;
using DiscordGames.Core.Memory;
using DiscordGames.Core.Memory.Pool;
using DiscordGames.Core.Net.Serialize;
using TMPro;
using UnityEngine;

public class Test : MonoBehaviour
{
    private void Start()
    {
        var sb = new StringBuilder();
        
        MemoryPool.Init(new SmallObjectHeapPool());
        
        var serialized =
            MessageSerializer.WritePingMessage(
                DiscordGames.Core.Net.MessageChannel.Direct, DateTime.UtcNow.Ticks);

        var str = string.Join(", ", serialized);
        sb.AppendLine($"<color=yellow>{str}</color>");
        
        var reader = new BufferReader(serialized);
        var header = reader.ReadHeader();
        var message = reader.ReadPingMessage(ref header);
        
        sb.AppendLine(new DateTime(message.UtcTicks, DateTimeKind.Utc).ToLocalTime().ToString(CultureInfo.InvariantCulture));

        GetComponent<TMP_Text>().text = sb.ToString();
    }
}
