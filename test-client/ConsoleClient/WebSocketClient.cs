using System.Threading.Channels;
using DiscordGames.Core;
using DiscordGames.Core.Net;
using DiscordGames.Core.Net.Message;
using DiscordGames.Core.Net.Serialize;
using PooledAwait;
using TestClient.Net;

namespace ConsoleClient;

public class WebSocketClient : IMessageHandler, IAsyncDisposable
{
    private readonly WebSocketWrapper wrapper;
    private readonly CancellationTokenSource cancellationTokenSource;
    private readonly Channel<byte[]> sendDataChannel;

    private bool isDisposed;
    private long lastServerPingTicks = -1;
    private bool hasLoggedIn = false;
    private UserId userId;

    public WebSocketClient(string host)
    {
        this.wrapper = new WebSocketWrapper(this);
        this.cancellationTokenSource = new CancellationTokenSource();
        this.sendDataChannel = Channel.CreateUnbounded<byte[]>();

        this.wrapper.OnOpen += this.OnOpen;

        _ = this.Run(host, this.cancellationTokenSource.Token);
    }

    private void ReserveSend(byte[] data)
    {
        this.sendDataChannel.Writer.TryWrite(data);
    }
    
    private void OnOpen()
    {
        Console.WriteLine("Connected!");

        this.ReserveSend(MessageSerializer.WriteGreetingMessage(MessageChannel.Direct, -1, Constants.BotAccessToken));
    }

    private async PooledTask Run(string host, CancellationToken cancellationToken)
    {
        await this.wrapper.Connect(new Uri(host), cancellationToken);
        var send = this.ProcessSend(cancellationToken);
        var recv = this.wrapper.ProcessReceive(cancellationToken);

        await Task.WhenAny(send, recv);
        Console.WriteLine("Disconnected!");
        await Task.WhenAll(send, recv);
    }

    private void SendPing()
    {
        this.ReserveSend(MessageSerializer.WritePingMessage(MessageChannel.Direct, DateTime.UtcNow.Ticks));
    }

    private async PooledTask ProcessSend(CancellationToken cancellationToken)
    {
        await foreach (var buffer in this.sendDataChannel.Reader.ReadAllAsync(cancellationToken))
        {
            try
            {
                if (cancellationToken.IsCancellationRequested) break;

                await this.wrapper.SendAsync(buffer, cancellationToken);
            }
            catch (OperationCanceledException) { }
        }
    }

    public async PooledValueTask Disconnect()
    {
        this.sendDataChannel.Writer.TryComplete();
        await this.sendDataChannel.Reader.Completion;
        await this.cancellationTokenSource.CancelAsync();
        await this.wrapper.Disconnect(CancellationToken.None);
    }

    public async ValueTask DisposeAsync()
    {
        if (this.isDisposed) return;
        
        GC.SuppressFinalize(this);
        
        await this.Disconnect();
        this.wrapper.Dispose();
        
        this.isDisposed = true;
    }

    public void HostPerudo(int maxPlayers, bool isClassicRule)
    {
        this.ReserveSend(MessageSerializer.WriteHostGameMessage(
            MessageChannel.Direct,
            Guid.NewGuid().ToString(),
            new PerudoHostGameData(maxPlayers, isClassicRule)
        ));
    }

    public ValueTask OnGreeting(GreetingMessage message)
    {
        if (this.hasLoggedIn)
        {
            Console.WriteLine("Already logged in");
            return ValueTask.CompletedTask;
        }
    
        this.userId = message.UserId;
        this.hasLoggedIn = true;
        this.SendPing();
        return ValueTask.CompletedTask;
    }

    public ValueTask OnPing(PingMessage message)
    {
        this.lastServerPingTicks = message.UtcTicks;
        this.SendPing();
        return ValueTask.CompletedTask;
    }

    public ValueTask OnHostGame(HostGameMessage message)
    {
        CoreThrowHelper.ThrowInvalidOperation();
        return ValueTask.CompletedTask;
    }
}