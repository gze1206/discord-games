using System.Text.Json.Serialization;
using DiscordGames.Grain.Implements.GameSessions;

namespace DiscordGames.Grain.States;

using static PerudoSessionGrain;
using PlayerNode = LinkedListNode<PerudoSessionGrain.PlayerInfo>;

// ReSharper disable once ClassNeverInstantiated.Global
[GenerateSerializer]
[Alias("DiscordGames.Grain.States.PerudoSessionState")]
public class PerudoSessionState
{
    [Id(0)]
    public HashSet<UserId> Players { get; } = new();
    [Id(1)]
    public HashSet<UserId> Spectators { get; } = new();
    [Id(2)]
    public LinkedList<PlayerInfo> TurnOrder { get; } = new();

    [Id(3)]
    public UserId HostUserId { get; internal set; }
    [Id(4)]
    public bool IsInitialized { get; internal set; }
    [Id(5)]
    public bool IsPlaying { get; internal set; }
    [Id(6)]
    public bool IsClassicRule { get; internal set; }
    [Id(7)]
    public int MaxPlayer { get; internal set; } = DefaultMaxPlayer;
    
    [field: NonSerialized, JsonIgnore]
    public PlayerNode? CurrentTurn { get; internal set; }
}