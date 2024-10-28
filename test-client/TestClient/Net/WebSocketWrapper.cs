// using System.Diagnostics;
// using DiscordGames.Core.Net;
// using DiscordGames.Core.Net.Serialize;
// using WebSocketSharp.NetCore;
//
// namespace TestClient.Net;
//
// public class WebSocketWrapper : IDisposable
// {
//     public event Action OnOpen = () => { };
//     
//     private readonly WebSocket socket;
//     private readonly IMessageHandler handler;
//
//     public WebSocketWrapper(string url, IMessageHandler handler)
//     {
//         this.socket = new WebSocket(url);
//         this.handler = handler;
//         this.socket.OnMessage += this.OnMessage;
//         this.socket.OnOpen += (_, _) => OnOpen();
//     }
//
//     public void Connect()
//     {
//         this.socket.Connect();
//     }
//
//     public void Dispose()
//     {
//         this.socket.Close();
//         ((IDisposable)this.socket).Dispose();
//         GC.SuppressFinalize(this);
//     }
//
//     public void Send(byte[] data)
//     {
//         this.socket.Send(data);
//     }
//
//     private void OnMessage(object? sender, MessageEventArgs e)
//     {
//         if (!e.IsBinary)
//         {
//             Debug.WriteLine(e.IsText ? e.Data : "(ping)");
//             return;
//         }
//         MessageSerializer.Read(e.RawData, this.handler);
//     }
// }