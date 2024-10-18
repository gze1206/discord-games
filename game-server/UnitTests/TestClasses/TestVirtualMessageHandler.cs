using DiscordGames.Core.Net;
using DiscordGames.Core.Net.Message;

namespace UnitTests.TestClasses;

public class TestVirtualMessageHandler : IMessageHandler
{
    public virtual Task OnPing(PingMessage message) { return Task.CompletedTask; }
}