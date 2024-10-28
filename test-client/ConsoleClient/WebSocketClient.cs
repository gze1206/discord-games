// using System.Diagnostics;
// using DiscordGames.Core;
// using DiscordGames.Core.Net;
// using DiscordGames.Core.Net.Message;
// using DiscordGames.Core.Net.Serialize;
// using TestClient.Net;
//
// namespace ConsoleClient;
//
// public class WebSocketClient : IMessageHandler, IDisposable
// {
//     private readonly WebSocketWrapper wrapper;
//     
//     private long lastServerPingTicks = -1;
//     private bool hasLoggedIn = false;
//     private UserId userId;
//
//     public WebSocketClient(string url)
//     {
//         this.wrapper = new(url, this);
//         this.wrapper.OnOpen += this.OnOpen;
//         this.wrapper.Connect();
//     }
//
//     private void OnOpen()
//     {
//         Console.WriteLine("Connected!");
//         this.wrapper.Send(MessageSerializer.WriteGreetingMessage(MessageChannel.Direct, -1, Constants.MockDiscordUid));
//     }
//
//     private void SendPing()
//     {
//         this.wrapper.Send(MessageSerializer.WritePingMessage(MessageChannel.Direct, DateTime.UtcNow.Ticks));
//     }
//     
//     public void Dispose()
//     {
//         this.wrapper.Dispose();
//         GC.SuppressFinalize(this);
//     }
//
//     public void HostPerudo(int maxPlayers, bool isClassicRule)
//         => this.wrapper.Send(MessageSerializer.WriteHostGameMessage(
//             MessageChannel.Direct,
//             Guid.NewGuid().ToString(),
//             new PerudoHostGameData(maxPlayers, isClassicRule)
//         ));
//     
//     public ValueTask OnGreeting(GreetingMessage message)
//     {
//         if (this.hasLoggedIn)
//         {
//             Console.WriteLine("Already logged in");
//             return ValueTask.CompletedTask;
//         }
//         
//         this.userId = message.UserId;
//         this.hasLoggedIn = true;
//         this.SendPing();
//
//         return ValueTask.CompletedTask;
//     }
//
//     public ValueTask OnPing(PingMessage message)
//     {
//         // if (0 <= this.lastServerPingTicks)
//         // {
//         //     var diff = message.UtcTicks - this.lastServerPingTicks;
//         //     Debug.WriteLine($"PING : {diff / TimeSpan.TicksPerMillisecond}ms");
//         // }
//
//         this.lastServerPingTicks = message.UtcTicks;
//         this.SendPing();
//         
//         return ValueTask.CompletedTask;
//     }
//
//     public ValueTask OnHostGame(HostGameMessage message) => throw new InvalidOperationException();
// }