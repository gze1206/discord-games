namespace DiscordGames.Grains.Interfaces;

[Alias("DiscordGames.Grain.Interfaces.IAuthGrain")]
public interface IAuthGrain : IGrainWithIntegerKey
{
    [Alias("GetAccessToken")]
    ValueTask<string> GetAccessToken(string authCode);

    [Alias("VerifyTokenAndGetUserId")]
    ValueTask<UserId> VerifyTokenAndGetUserId(string accessToken);
}