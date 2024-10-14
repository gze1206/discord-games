using DiscordGames.Grain.Interfaces.GameSessions;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace DiscordGames.Server.Net;

public class MessageHandler : WebSocketBehavior
{
    protected override void OnMessage(MessageEventArgs e)
    {
        var session = ServiceLocator.GrainFactory.GetGrain<IPerudoSessionGrain>(new Guid());
        var result = session.JoinPlayer(1).GetAwaiter().GetResult();
        this.Send(e.Data + result);
    }
}