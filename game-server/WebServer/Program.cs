using DiscordGames.Core.Memory.Pool;
using DiscordGames.Grains.Serialization;
using Orleans.Serialization;
using WebServer.Net;
using WebServer.Services;

MemoryPool.Init(new PinnedObjectHeapPool());

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://localhost:9000");

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHostedService<HealthCheckService>();

builder.Services.AddLogging(logging =>
{
    logging.AddSimpleConsole(options => options.IncludeScopes = true);
});

builder.Host.UseOrleansClient(client =>
{
    var jsonConvert = SerializeDefines.JsonConvertBuilder;

    client.Services
        .AddSerializer(serializer => serializer.AddJsonSerializer(
            isSupported: jsonConvert.IsSupport,
            jsonSerializerOptions: jsonConvert.BakeOptions()));
    
    client.UseLocalhostClustering();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseWebSockets();
app.UseHttpsRedirection();

app.Use(async (context, next) =>
{
    if (context.Request.Path == "/ws")
    {
        if (context.WebSockets.IsWebSocketRequest)
        {
            using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
            var conn = ConnectionPool.I.Rent(
                context.RequestServices.GetRequiredService<ILogger<Connection>>(),
                context.RequestServices.GetRequiredService<IClusterClient>());

            var address = "(Unknown)";
            if (context.Connection.RemoteIpAddress != null)
            {
                address = $"{context.Connection.RemoteIpAddress}:{context.Connection.RemotePort}";
            }
            
            conn.Initialize(webSocket, address);
            
            await conn.Loop();
            
            ConnectionPool.I.Return(conn);
        }
        else
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
        }
    }
    else
    {
        await next();
    }
});

app.Run();
