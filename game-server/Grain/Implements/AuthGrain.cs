using DiscordGames.Grains.Interfaces;
using Orleans.Concurrency;
using PooledAwait;

using static DiscordGames.Core.Constants;
using static DiscordGames.Grains.Constants;

namespace DiscordGames.Grains.Implements;

[StatelessWorker]
public class AuthGrain : Grain, IAuthGrain
{
    public ValueTask<string> GetAccessToken(string authCode)
    {
        return Internal(authCode);
        static async PooledValueTask<string> Internal(string authCode)
        {
            if (authCode == BotAuthCode) return BotAccessToken;

            //TODO: 실제 Discord API를 적용해야 합니다
            return authCode;
        }
    }

    public ValueTask<UserId> VerifyTokenAndGetUserId(string accessToken)
    {
        return Internal(this.GrainFactory, accessToken);
        static async PooledValueTask<UserId> Internal(IGrainFactory factory, string accessToken)
        {
            if (accessToken == BotAccessToken)
            {
                var botManager = factory.GetGrain<IBotManagerGrain>(SingletonGrainId);
                return await botManager.RentBotUserId();
            }
            
            //TODO: 검증 진행 후 실제 UserId를 찾아 반환해야 합니다
            return -1;
        }
    }
}