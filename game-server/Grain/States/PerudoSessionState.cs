using System.Text.Json.Serialization;
using DiscordGames.Grain.Implements.GameSessions;

namespace DiscordGames.Grain.States;

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
    [Id(4)] public bool IsInitialized { get; internal set; }
    [Id(5)] public bool IsPlaying { get; internal set; }
    [Id(6)] public bool IsClassicRule { get; internal set; }
    [Id(7)] public int MaxPlayer { get; internal set; } = DefaultMaxPlayer;
    [Id(8)] public int CurrentTurn { get; internal set; } = -1;
    [Id(9)] public UserId LastBidUserId { get; internal set; } = -1;
    [Id(10)] public int LastBidQuantity { get; internal set; } = -1;
    [Id(11)] public int LastBidFace { get; internal set; } = -1;

    public PerudoSessionState()
    {
    }

    [JsonConstructor]
    public PerudoSessionState(HashSet<UserId> players,
        HashSet<UserId> spectators, 
        List<PlayerInfo> turnOrder,
        int hostUserId,
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