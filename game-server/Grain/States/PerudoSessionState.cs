using System.Text.Json.Serialization;
using DiscordGames.Grains.Implements.GameSessions;

namespace DiscordGames.Grains.States;

using static PerudoSessionGrain;

// ReSharper disable once ClassNeverInstantiated.Global
[GenerateSerializer]
[Alias("DiscordGames.Grain.States.PerudoSessionState")]
public class PerudoSessionState
{
    [Id(0)] public HashSet<UserId> Players { get; } = new();
    [Id(1)] public HashSet<UserId> Spectators { get; } = new();
    [Id(2)] public List<PlayerInfo> TurnOrder { get; } = new();
    
    [Id(3)] public UserId HostUserId { get; internal set; }
    [Id(4)] public string? SessionName { get; internal set; }
    [Id(5)] public bool IsInitialized { get; internal set; }
    [Id(6)] public bool IsPlaying { get; internal set; }
    [Id(7)] public bool IsClassicRule { get; internal set; }
    [Id(8)] public int MaxPlayer { get; internal set; } = DefaultMaxPlayer;
    [Id(9)] public int CurrentTurn { get; internal set; } = -1;
    [Id(10)] public UserId LastBidUserId { get; internal set; } = -1;
    [Id(11)] public int LastBidQuantity { get; internal set; } = -1;
    [Id(12)] public int LastBidFace { get; internal set; } = -1;

    public PerudoSessionState() { }

    [JsonConstructor]
    public PerudoSessionState(HashSet<UserId> players,
        HashSet<UserId> spectators, 
        List<PlayerInfo> turnOrder,
        int hostUserId,
        string sessionName,
        bool isInitialized,
        bool isPlaying,
        bool isClassicRule,
        int maxPlayer,
        int currentTurn,
        UserId lastBidUserId,
        int lastBidQuantity,
        int lastBidFace)
    {
        this.Players = players;
        this.Spectators = spectators;
        this.TurnOrder = turnOrder;
        this.HostUserId = hostUserId;
        this.SessionName = sessionName;
        this.IsInitialized = isInitialized;
        this.IsPlaying = isPlaying;
        this.IsClassicRule = isClassicRule;
        this.MaxPlayer = maxPlayer;
        this.CurrentTurn = currentTurn;
        this.LastBidUserId = lastBidUserId;
        this.LastBidQuantity = lastBidQuantity;
        this.LastBidFace = lastBidFace;
    }
}