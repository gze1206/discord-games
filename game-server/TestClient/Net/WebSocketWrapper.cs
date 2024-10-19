using DiscordGames.Core.Net;
using DiscordGames.Core.Net.Message;
using WebSocketSharp;

namespace TestClient.Net;

public class WebSocketWrapper : IDisposable
{
    private readonly WebSocket socket;
    private readonly IMessageHandler handler;

    public WebSocketWrapper(string url, IMessageHandler handler)
    {
        this.socket = new WebSocket(url);
        this.handler = handler;
        this.socket.OnMessage += this.OnMessage;
        this.socket.Connect();
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        ((IDisposable)this.socket).Dispose();
    }

    public void Send(byte[] data)
    {
        this.socket.Send(data);
    }

    private void OnMessage(object? sender, MessageEventArgs e)
    {
        MessageSerializer.Read(e.RawData, this.handler);
    }
}