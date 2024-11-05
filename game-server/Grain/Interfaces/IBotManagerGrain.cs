namespace DiscordGames.Grains.Interfaces;

[Alias("DiscordGames.Grains.Interfaces.IBotManagerGrain")]
public interface IBotManagerGrain : IGrainWithIntegerKey
{
    [Alias("RentBotUserId")] ValueTask<UserId> RentBotUserId();
    
    [Alias("ReturnBotUserId")] ValueTask ReturnBotUserId(UserId id);
}