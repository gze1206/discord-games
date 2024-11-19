using DiscordGames.Grains.Interfaces;
using DiscordGames.Grains.States;

namespace DiscordGames.Grains.Implements;

public class UserGrain : Grain<UserState>, IUserGrain
{
    private Queue<byte[]> sendQueue = default!;

    public override Task OnActivateAsync(CancellationToken cancellationToken)
    {
        this.sendQueue = new Queue<byte[]>();
        return Task.CompletedTask;
    }

    public Task<UserState> GetState() => Task.FromResult(this.State);
    
    public ValueTask<bool> IsConnected() => ValueTask.FromResult(this.State.IsConnected);
    public ValueTask<string?> GetSessionUid() => ValueTask.FromResult(this.State.PlayingSessionId);

    public ValueTask SetSessionUid(string? newSessionUid)
    {
        this.State.PlayingSessionId = newSessionUid;
        return ValueTask.CompletedTask;
    }

    public ValueTask SetConnect(bool connected)
    {
        if (connected)
        {
            if (this.State.IsConnected) return ValueTask.FromException(GrainThrowHelper.AlreadyConnectedUser);
        
            this.sendQueue.Clear();
            this.State.PlayingSessionId = null;
            this.State.IsConnected = true;
        }
        else
        {
            if (!this.State.IsConnected) return ValueTask.FromException(GrainThrowHelper.AlreadyDisconnectedUser);

            this.State.IsConnected = false;
        }

        return ValueTask.CompletedTask;
    }

    public ValueTask ReserveSend(byte[] data)
    {
        if (!this.State.IsConnected) return ValueTask.CompletedTask;
        if (data.Length <= 0) return ValueTask.FromException(CoreThrowHelper.InvalidOperation);
        
        this.sendQueue.Enqueue(data);
        return ValueTask.CompletedTask;
    }

    public ValueTask<byte[][]> GetAndClearQueue()
    {
        if (!this.State.IsConnected) return ValueTask.FromResult(Array.Empty<byte[]>());
        
        var queue = this.sendQueue.ToArray();
        this.sendQueue.Clear();
        return ValueTask.FromResult(queue);
    }
}