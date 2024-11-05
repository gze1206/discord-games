using DiscordGames.Grains.Interfaces;
using PooledAwait;
using static DiscordGames.Core.Constants;
using static DiscordGames.Grains.Constants;

namespace DiscordGames.Grains.Implements;

public class BotManagerGrain : Grain, IBotManagerGrain, IRemindable
{
    private const string ReminderName = "BotManagerKeepAlive";
    
    private Queue<UserId> botUserIdPool = default!;
    
    public Task ReceiveReminder(string reminderName, TickStatus status) => Task.CompletedTask;
    
    public override Task OnActivateAsync(CancellationToken cancellationToken)
    {
        return Internal(this, cancellationToken);
        static async PooledTask Internal(BotManagerGrain self, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
        
            if (self.GetPrimaryKeyLong() != SingletonGrainId)
                GrainThrowHelper.ThrowInvalidSingletonGrainKey();

            self.botUserIdPool = new Queue<int>(BotUserCount);

            for (var i = 1; i <= BotUserCount; i++)
            {
                self.botUserIdPool.Enqueue(i);
            }

            await self.RegisterOrUpdateReminder(ReminderName, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
        }
    }

    public ValueTask<UserId> RentBotUserId()
    {
        return this.botUserIdPool.Count <= 0
            ? ValueTask.FromException<int>(GrainThrowHelper.BotUserPoolExhausted)
            : ValueTask.FromResult(this.botUserIdPool.Dequeue());
    }

    public ValueTask ReturnBotUserId(UserId id)
    {
        if (BotUserCount < id)
            return ValueTask.FromException(GrainThrowHelper.InvalidBotUser);
        
        this.botUserIdPool.Enqueue(id);
        return ValueTask.CompletedTask;
    }
}