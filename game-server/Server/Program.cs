﻿using System.Net;
using DiscordGames.Server;
using DiscordGames.Server.Net;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WebSocketSharp.Server;

var webSocketServer = new WebSocketServer(9000);

try
{
    webSocketServer.AddWebSocketService<MessageHandler>("/");
    webSocketServer.Start();
    
    var builder = Host.CreateDefaultBuilder(args)
        .UseOrleans(silo =>
        {
            silo.UseLocalhostClustering()
                .ConfigureLogging(logging => logging.AddConsole());

            silo.UseDashboard(options => options.HostSelf = true);
        })
        .UseConsoleLifetime();

    builder.ConfigureServices(services => services.AddLogging(conf => conf.AddConsole()));

    using var host = builder.Build();

    ServiceLocator.GrainFactory = host.Services.GetRequiredService<IGrainFactory>();
    Console.WriteLine("WebSocket Listening on {0}:{1}...", webSocketServer.Address ?? IPAddress.Loopback, webSocketServer.Port);

    await host.RunAsync();
}
finally
{
    webSocketServer.Stop();
}
