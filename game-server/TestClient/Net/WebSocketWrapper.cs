using DiscordGames.Core.Net;
using DiscordGames.Core.Net.Serialize;
using WebSocketSharp;

namespace TestClient.Net;

public class WebSocketWrapper : IDisposable
{
    public event Action OnOpen = () => { };
    
    private readonly WebSocket socket;
    private readonly IMessageHandler handler;

    public WebSocketWrapper(string url, IMessageHandler handler)
    {
        this.socket = new WebSocket(url);
        this.handler = handler;
        this.socket.OnMessage += this.OnMessage;
        this.socket.OnOpen += (_, _) => OnOpen();
    }

    public void Connect()
    {
        this.socket.Connect();
    }

    public void Dispose()
    {
        ((IDisposable)this.socket).Dispose();
        GC.SuppressFinalize(this);
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