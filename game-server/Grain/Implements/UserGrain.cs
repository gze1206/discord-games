using DiscordGames.Grains.Interfaces;

namespace DiscordGames.Grains.Implements;

public class UserGrain : Grain, IUserGrain
{
    private Queue<byte[]> sendQueue = default!;

    public override Task OnActivateAsync(CancellationToken cancellationToken)
    {
        this.sendQueue = new Queue<byte[]>();
        return Task.CompletedTask;
    }

    public ValueTask ReserveSend(byte[] data)
    {
        if (data.Length <= 0) return ValueTask.FromException(CoreThrowHelper.InvalidOperation);
        
        this.sendQueue.Enqueue(data);
        return ValueTask.CompletedTask;
    }

    public ValueTask<byte[][]> GetAndClearQueue()
    {
        var queue = this.sendQueue.ToArray();
        this.sendQueue.Clear();
        return ValueTask.FromResult(queue);
    }
}