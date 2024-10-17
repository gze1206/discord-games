using System.Text.Json.Serialization;
using DiscordGames.Grain.Implements.GameSessions;

namespace DiscordGames.Grain.States;

using static PerudoSessionGrain;

// ReSharper disable once ClassNeverInstantiated.Global
[Alias("DiscordGames.Grain.States.PerudoSessionState")]
public class PerudoSessionState
{
    public HashSet<UserId> Players { get; } = new();
    public HashSet<UserId> Spectators { get; } = new();
    public LinkedList<PlayerInfo> TurnOrder { get; } = new();

    public UserId HostUserId { get; internal set; }
    public bool IsInitialized { get; internal set; }
    public bool IsPlaying { get; internal set; }
    public bool IsClassicRule { get; internal set; }
    public int MaxPlayer { get; internal set; } = DefaultMaxPlayer;
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