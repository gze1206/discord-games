using System.Collections.Concurrent;
using DiscordGames.Core;
using DiscordGames.Core.Net;
using DiscordGames.Core.Net.Message;
using DiscordGames.Core.Net.Serialize;
using PooledAwait;
using TestClient.Net;

namespace ConsoleClient;

public class WebSocketClient : IMessageHandler, IDisposable
{
    private readonly WebSocketWrapper wrapper;
    private readonly CancellationTokenSource cancellationTokenSource;
    private readonly ConcurrentQueue<byte[]> sendQueue;

    private long lastServerPingTicks = -1;
    private bool hasLoggedIn = false;
    private UserId userId;

    public WebSocketClient(string host)
    {
        this.wrapper = new WebSocketWrapper(this);
        this.cancellationTokenSource = new CancellationTokenSource();
        this.sendQueue = new ConcurrentQueue<byte[]>();
        
        this.wrapper.OnOpen += this.OnOpen;

        Task.Run(this.ProcessSend, this.cancellationTokenSource.Token);
        Task.Run(async () =>
        {
            await this.wrapper.Connect(new Uri(host), this.cancellationTokenSource.Token);
            await this.wrapper.Loop(this.cancellationTokenSource.Token);
            
        }, this.cancellationTokenSource.Token);
    }

    private void OnOpen()
    {
        Console.WriteLine("Connected!");
        this.sendQueue.Enqueue(MessageSerializer.WriteGreetingMessage(MessageChannel.Direct, -1, Constants.MockDiscordUid));
    }

    private void SendPing() => this.sendQueue.Enqueue(MessageSerializer.WritePingMessage(MessageChannel.Direct, DateTime.UtcNow.Ticks));

    private ValueTask ProcessSend()
    {
        return Internal(this, this.cancellationTokenSource.Token);
        static async PooledValueTask Internal(WebSocketClient self, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested || !self.sendQueue.IsEmpty)
            {
                var buffers = new List<byte[]>();
                while (self.sendQueue.TryDequeue(out var buffer))
                {
                    buffers.Add(buffer);
                }

                foreach (var buffer in buffers)
                {
                    await self.wrapper.SendAsync(buffer);
                }
            }
        }
    }

    public ValueTask Disconnect()
    {
        return Internal(this);
        static async PooledValueTask Internal(WebSocketClient self)
        {
            await self.cancellationTokenSource.CancelAsync();
            await self.wrapper.Disconnect(CancellationToken.None);
        }
    }

    public void Dispose()
    {
        var disconnectTask = this.Disconnect();
        if (!disconnectTask.IsCompleted) disconnectTask.AsTask().GetAwaiter().GetResult();

        var disposeTask = this.wrapper.DisposeAsync();
        if (!disposeTask.IsCompleted) disposeTask.AsTask().GetAwaiter().GetResult();

        GC.SuppressFinalize(this);
    }

    public void HostPerudo(int maxPlayers, bool isClassicRule)
        => this.sendQueue.Enqueue(MessageSerializer.WriteHostGameMessage(
            MessageChannel.Direct,
            Guid.NewGuid().ToString(),
            new PerudoHostGameData(maxPlayers, isClassicRule)
        ));
    
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
        // if (0 <= this.lastServerPingTicks)
        // {
        //     var diff = message.UtcTicks - this.lastServerPingTicks;
        //     Debug.WriteLine($"PING : {diff / TimeSpan.TicksPerMillisecond}ms");
        // }

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