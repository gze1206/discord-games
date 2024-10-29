using DiscordGames.Core.Net;
using DiscordGames.Core.Net.Message;

namespace WebServer.Net;

public partial class Connection : IMessageHandler
{
    public ValueTask OnGreeting(GreetingMessage message)
    {
        this.logger.LogInformation("GREETING [{discordUid}]", message.DiscordUid);
        
        return ValueTask.CompletedTask;
    }

    public ValueTask OnPing(PingMessage message)
    {
        this.logger.LogInformation("PING [{ticks}]", message.UtcTicks);
        
        return ValueTask.CompletedTask;
    }

    public ValueTask OnHostGame(HostGameMessage message)
    {
        this.logger.LogInformation("HOST [{name}]", message.Name);
        
        return ValueTask.CompletedTask;
    }
}