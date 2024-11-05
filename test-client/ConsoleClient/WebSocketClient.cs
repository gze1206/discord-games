using System.Collections.Concurrent;
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
    private readonly Queue<byte[]> sendQueue;

    private bool isDisposed;
    private long lastServerPingTicks = -1;
    private bool hasLoggedIn = false;
    private UserId userId;
    private SpinLock sendQueueLock;

    public WebSocketClient(string host)
    {
        this.wrapper = new WebSocketWrapper(this);
        this.cancellationTokenSource = new CancellationTokenSource();
        this.sendQueue = new Queue<byte[]>();
        
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

        var lockTaken = false;
        try
        {
            this.sendQueueLock.TryEnter(ref lockTaken);
            this.sendQueue.Enqueue(MessageSerializer.WriteGreetingMessage(MessageChannel.Direct, -1, Constants.BotAccessToken));
        }
        finally
        {
            if (lockTaken) this.sendQueueLock.Exit();
        }
    }

    private void SendPing()
    {
        var lockTaken = false;
        try
        {
            this.sendQueueLock.TryEnter(ref lockTaken);
            this.sendQueue.Enqueue(MessageSerializer.WritePingMessage(MessageChannel.Direct, DateTime.UtcNow.Ticks));
        }
        finally
        {
            if (lockTaken) this.sendQueueLock.Exit();
        }
    }

    private ValueTask ProcessSend()
    {
        return Internal(this, this.cancellationTokenSource.Token);
        static async PooledValueTask Internal(WebSocketClient self, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var lockTaken = false;
                byte[][]? buffers;
                try
                {
                    self.sendQueueLock.TryEnter(ref lockTaken);
                    buffers = self.sendQueue.ToArray();
                    self.sendQueue.Clear();
                }
                finally
                {
                    if (lockTaken) self.sendQueueLock.Exit();
                }

                foreach (var buffer in buffers)
                {
                    await self.wrapper.SendAsync(buffer);
                }

                await Task.Delay(1000, cancellationToken);
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

    public async ValueTask DisposeAsync()
    {
        if (this.isDisposed) return;
        
        GC.SuppressFinalize(this);
        
        await this.Disconnect();
        await this.wrapper.DisposeAsync();
        
        this.isDisposed = true;
    }

    public void HostPerudo(int maxPlayers, bool isClassicRule)
    {
        var lockTaken = false;
        try
        {
            this.sendQueueLock.TryEnter(ref lockTaken);
            this.sendQueue.Enqueue(MessageSerializer.WriteHostGameMessage(
                MessageChannel.Direct,
                Guid.NewGuid().ToString(),
                new PerudoHostGameData(maxPlayers, isClassicRule)
            ));
        }
        finally
        {
            if (lockTaken) this.sendQueueLock.Exit();
        }
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