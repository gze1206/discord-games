using DiscordGames.Server.Serialization.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

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

await host.RunAsync();
