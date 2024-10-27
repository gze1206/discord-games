using ConsoleClient;
using DiscordGames.Core.Memory.Pool;

MemoryPool.Init(new PinnedObjectHeapPool());

var socket = new WebSocketClient("ws://localhost:9000");

while (Console.ReadLine() is { } cmd)
{
    if (cmd == "q") break;
}

socket.Dispose();