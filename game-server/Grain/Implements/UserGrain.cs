using DiscordGames.Grains.Interfaces;

namespace DiscordGames.Grains.Implements;

public class UserGrain : Grain, IUserGrain
{
    private Queue<byte[]> sendQueue = default!;
    private string? sessionUid;
    private bool isConnected;

    public override Task OnActivateAsync(CancellationToken cancellationToken)
    {
        this.sendQueue = new Queue<byte[]>();
        return Task.CompletedTask;
    }
    
    public ValueTask<bool> IsConnected() => ValueTask.FromResult(this.isConnected);
    public ValueTask<string?> GetSessionUid() => ValueTask.FromResult(this.sessionUid);

    public ValueTask SetSessionUid(string? newSessionUid)
    {
        this.sessionUid = newSessionUid;
        return ValueTask.CompletedTask;
    }

    public ValueTask SetConnect(bool connected)
    {
        if (connected)
        {
            if (this.isConnected) return ValueTask.FromException(GrainThrowHelper.AlreadyConnectedUser);
        
            this.sendQueue.Clear();
            this.sessionUid = null;
            this.isConnected = true;
        }
        else
        {
            if (!this.isConnected) return ValueTask.FromException(GrainThrowHelper.AlreadyDisconnectedUser);

            this.isConnected = false;
        }

        return ValueTask.CompletedTask;
    }

    public ValueTask ReserveSend(byte[] data)
    {
        if (!this.isConnected) return ValueTask.CompletedTask;
        if (data.Length <= 0) return ValueTask.FromException(CoreThrowHelper.InvalidOperation);
        
        this.sendQueue.Enqueue(data);
        return ValueTask.CompletedTask;
    }

    public ValueTask<byte[][]> GetAndClearQueue()
    {
        if (!this.isConnected) return ValueTask.FromResult(Array.Empty<byte[]>());
        
        var queue = this.sendQueue.ToArray();
        this.sendQueue.Clear();
        return ValueTask.FromResult(queue);
    }
}