using DiscordGames.Core.Net;
using DiscordGames.Core.Net.Message;

// ReSharper disable once CheckNamespace
namespace UnitTests.TestClasses;

public class TestVirtualMessageHandler : IMessageHandler
{
    public virtual Task OnPing(PingMessage message) { return Task.CompletedTask; }

    public virtual Task OnGreeting(GreetingMessage message) { return Task.CompletedTask; }
}