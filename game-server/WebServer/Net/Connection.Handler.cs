using DiscordGames.Core.Net;
using DiscordGames.Core.Net.Message;

namespace WebServer.Net;

public partial class Connection : IMessageHandler
{
    public ValueTask OnGreeting(GreetingMessage message)
    {
        throw new NotImplementedException();
    }

    public ValueTask OnPing(PingMessage message)
    {
        throw new NotImplementedException();
    }

    public ValueTask OnHostGame(HostGameMessage message)
    {
        throw new NotImplementedException();
    }
}