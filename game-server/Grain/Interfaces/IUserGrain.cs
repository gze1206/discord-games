using DiscordGames.Grains.States;

namespace DiscordGames.Grains.Interfaces;

[Alias("DiscordGames.Grains.Interfaces.IUserGrain")]
public interface IUserGrain : IGrainWithIntegerKey
{
    [Alias("GetSessionUid")] ValueTask<string?> GetSessionUid();
    [Alias("SetSessionUid")] ValueTask SetSessionUid(string? newSessionUid);
    
    [Alias("IsConnected")] ValueTask<bool> IsConnected();
    [Alias("SetConnect")] ValueTask SetConnect(bool connected);
    
    [Alias("ReserveSend")] ValueTask ReserveSend(byte[] data);
    
    [Alias("GetAndClearQueue")] ValueTask<byte[][]> GetAndClearQueue();
    
    [Alias("GetState")] Task<UserState> GetState();
}