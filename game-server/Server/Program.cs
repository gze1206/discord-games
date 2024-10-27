using System.Net;
using DiscordGames.Core.Memory.Pool;
using DiscordGames.Server;
using DiscordGames.Server.Net;
using DiscordGames.Server.Serialization.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WebSocketSharp.NetCore.Server;

var webSocketServer = new WebSocketServer(9000);

try
{
    MemoryPool.Init(new PinnedObjectHeapPool());
    
    webSocketServer.AddWebSocketService<MessageHandler>("/");
    webSocketServer.Start();
    
    var builder = Host.CreateDefaultBuilder(args)
        .UseOrleans(silo =>
        {
            var jsonConvert = new CustomJsonConvertBuilder()
                .Add<LinkedListJsonConverter<int>>();
            
            silo.UseLocalhostClustering();

            silo
                .AddCustomJsonSerializer(jsonConvert)
                .UseDashboard(options => options.HostSelf = true);
        })
        .UseConsoleLifetime();

    builder.ConfigureServices(services => services
        .AddLogging(conf => conf
            .AddSimpleConsole(options => options.IncludeScopes = true)));

    using var host = builder.Build();

    ServiceLocator.GrainFactory = host.Services.GetRequiredService<IGrainFactory>();
    ServiceLocator.LoggerFactory = LoggerFactory.Create(loggingBuilder =>
    {
        loggingBuilder.AddSimpleConsole(options => options.IncludeScopes = true);
    });
    
    Console.WriteLine("WebSocket Listening on {0}:{1}...", webSocketServer.Address ?? IPAddress.Loopback, webSocketServer.Port.ToString());

    await host.RunAsync();
}
finally
{
    webSocketServer.Stop();
}
