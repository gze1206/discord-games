using System.Text.Json.Serialization;
using DiscordGames.Grain.Implements.GameSessions;

namespace DiscordGames.Grain.States;

using static PerudoSessionGrain;

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
    [Id(8)]
    public UserId? CurrentTurn { get; internal set; }
    
    public PerudoSessionState() { }
    
    [JsonConstructor]
    public PerudoSessionState(HashSet<UserId> players,
        HashSet<UserId> spectators,
        LinkedList<PlayerInfo> turnOrder,
        int hostUserId,
        bool isInitialized,
        bool isPlaying,
        bool isClassicRule,
        int maxPlayer,
        int? currentTurn)
    {
        this.Players = players;
        this.Spectators = spectators;
        this.TurnOrder = turnOrder;
        this.HostUserId = hostUserId;
        this.IsInitialized = isInitialized;
        this.IsPlaying = isPlaying;
        this.IsClassicRule = isClassicRule;
        this.MaxPlayer = maxPlayer;
        this.CurrentTurn = currentTurn;
    }
}