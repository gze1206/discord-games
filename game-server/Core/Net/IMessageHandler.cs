using System.Threading.Tasks;
using DiscordGames.Core.Net.Message;

namespace DiscordGames.Core.Net;

public interface IMessageHandler
{
    Task OnPing(PingMessage message);
    Task OnGreeting(GreetingMessage message);
}