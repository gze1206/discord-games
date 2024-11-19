using ConsoleClient;
using DiscordGames.Core.Memory.Pool;

MemoryPool.Init(new PinnedObjectHeapPool());

var socket = new WebSocketClient("ws://localhost:9000/ws");

try
{
    while (Console.ReadLine() is { } cmd)
    {
        if (string.IsNullOrWhiteSpace(cmd)) break;
        if (cmd == "q") break;

        var tokens = cmd.Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        switch (tokens[0].ToLower())
        {
            case "host":
                if (tokens.Length != 4) goto default;
                switch (tokens[1].ToLower())
                {
                    case "perudo":
                        socket.HostPerudo(tokens[2], tokens[3], 4, false);
                        continue;
                }

                goto default;
            case "edit":
                if (tokens.Length != 3) goto default;
                switch (tokens[1].ToLower())
                {
                    case "perudo":
                        socket.EditPerudo(tokens[2], 4, false);
                        continue;
                }

                goto default;
            default:
                Console.WriteLine("Unknown command");
                break;
        }
    }
}
catch (Exception e)
{
    Console.WriteLine(e.ToString());
}
finally
{
    await socket.Disconnect();
    await socket.DisposeAsync();
    
}
