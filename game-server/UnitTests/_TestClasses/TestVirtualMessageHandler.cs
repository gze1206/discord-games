using DiscordGames.Core.Net;
using DiscordGames.Core.Net.Message;

// ReSharper disable once CheckNamespace
namespace UnitTests.TestClasses;

public class TestVirtualMessageHandler : IMessageHandler
{
    public virtual ValueTask OnError(ErrorMessage message) => ValueTask.CompletedTask;
    public virtual ValueTask OnGreeting(GreetingMessage message) => ValueTask.CompletedTask;
    public virtual ValueTask OnPing(PingMessage message) => ValueTask.CompletedTask;

    public ValueTask OnHostGame(HostGameMessage message) => throw CoreThrowHelper.InvalidOperation;
    public ValueTask OnEditGame(EditGameMessage message) => throw CoreThrowHelper.InvalidOperation;
}