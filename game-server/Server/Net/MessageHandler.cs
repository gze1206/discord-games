using DiscordGames.Grain.Interfaces.GameSessions;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace DiscordGames.Server.Net;

public class MessageHandler : WebSocketBehavior
{
    protected override void OnMessage(MessageEventArgs e)
    {
        if (!int.TryParse(e.Data, out var userId)) userId = 0;
        var session = ServiceLocator.GrainFactory.GetGrain<IPerudoSessionGrain>(new Guid());
        var result = session.JoinPlayer(userId).GetAwaiter().GetResult();
        this.Send(e.Data + result);
    }
}