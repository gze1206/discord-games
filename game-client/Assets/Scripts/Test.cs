using System;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using DiscordGames.Core.Memory;
using DiscordGames.Core.Memory.Pool;
using DiscordGames.Core.Net;
using DiscordGames.Core.Net.Message;
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
            MessageSerializer.WritePingMessage(MessageChannel.Direct, DateTime.UtcNow.Ticks);

        var str = string.Join(", ", serialized);
        sb.AppendLine($"<color=yellow>{str}</color>");
        
        MessageSerializer.Read(serialized, new TestHandler(GetComponent<TMP_Text>(), sb));
    }
    
    private class TestHandler : IMessageHandler
    {
        private readonly TMP_Text text;
        private readonly StringBuilder sb;

        public TestHandler(TMP_Text text, StringBuilder sb)
        {
            this.text = text;
            this.sb = sb;
        }
        
        public ValueTask OnGreeting(GreetingMessage message)
        {
            throw new NotImplementedException();
        }

        public ValueTask OnPing(PingMessage message)
        {
            sb.AppendLine(new DateTime(message.UtcTicks, DateTimeKind.Utc).ToLocalTime()
                .ToString(CultureInfo.InvariantCulture));
            text.text = sb.ToString();
            return UniTask.CompletedTask.AsValueTask();
        }
    }
}

