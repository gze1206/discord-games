using System;

namespace DiscordGames.Core;

public static class Constants
{
    public const int BotUserCount = 2_000_000;
    public const ulong BotDiscordUid = 0;
    public const string BotAuthCode = "BOT_CODE";
    public const string BotAccessToken = "BOT_TOKEN";

    public const long TicksToLive = 60000 * TimeSpan.TicksPerMillisecond;
}