using Microsoft.Extensions.Logging;

namespace DiscordGames.Server;

public static class ServiceLocator
{
    public static IGrainFactory GrainFactory = default!;
    public static ILoggerFactory LoggerFactory = default!;
}