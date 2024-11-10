namespace DiscordGames.Grains.Interfaces;

[Alias("DiscordGames.Grains.Interfaces.IUserGrain")]
public interface IUserGrain : IGrainWithIntegerKey
{
    [Alias("GetSessionUid")] ValueTask<Guid> GetSessionUid();
    [Alias("SetSessionUid")] ValueTask SetSessionUid(Guid newSessionUid);
    
    [Alias("IsConnected")] ValueTask<bool> IsConnected();
    [Alias("SetConnect")] ValueTask SetConnect(bool connected);
    
    [Alias("ReserveSend")] ValueTask ReserveSend(byte[] data);
    
    [Alias("GetAndClearQueue")] ValueTask<byte[][]> GetAndClearQueue();
}