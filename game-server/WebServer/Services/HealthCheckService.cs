using PooledAwait;
using WebServer.LogMessages.Services;
using WebServer.Net;

using static DiscordGames.Core.Constants;

namespace WebServer.Services;

public class HealthCheckService : BackgroundService
{
    private static readonly TimeSpan Frequency = TimeSpan.FromMilliseconds(100);
    
    private readonly ILogger<HealthCheckService> logger;

    public HealthCheckService(ILogger<HealthCheckService> logger)
    {
        this.logger = logger;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var now = DateTime.UtcNow.Ticks;
                var connections = ConnectionPool.I.GetActiveConnections();

                foreach (var conn in connections)
                {
                    var diff = now - conn.LastActiveAtUtc;
                    if (diff <= TicksToLive) continue;

                    this.logger.LogOnClosing(conn.Address, conn.UserId, diff / (double)TimeSpan.TicksPerSecond);
                    await conn.Kill();
                }

                await Task.Delay(Frequency, stoppingToken);
            }
            catch (Exception e)
            {
                this.logger.LogCaughtException(e);
            }
        }
    }
}