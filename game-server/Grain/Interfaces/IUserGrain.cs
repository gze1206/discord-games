namespace DiscordGames.Grains.Interfaces;

[Alias("DiscordGames.Grains.Interfaces.IUserGrain")]
public interface IUserGrain : IGrainWithIntegerKey
{
    [Alias("ReserveSend")] ValueTask ReserveSend(byte[] data);
    
    [Alias("GetAndClearQueue")] ValueTask<byte[][]> GetAndClearQueue();
}