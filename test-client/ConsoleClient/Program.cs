using ConsoleClient;
using DiscordGames.Core.Memory.Pool;

MemoryPool.Init(new PinnedObjectHeapPool());

var socket = new WebSocketClient("ws://localhost:9000");

while (Console.ReadLine() is { } cmd)
{
    if (cmd == "q") break;

    var tokens = cmd.ToLower().Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
    switch (tokens[0])
    {
        case "host":
            if (tokens.Length != 2) continue;
            switch (tokens[1])
            {
                case "perudo":
                    socket.HostPerudo(4, false);
                    continue;
            }
            goto default;
        default:
            Console.WriteLine("Unknown command");
            break;
    }
}

socket.Dispose();