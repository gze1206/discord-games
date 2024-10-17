using System.Text.Json.Serialization;
using DiscordGames.Core;
using DiscordGames.Grain.Interfaces.GameSessions;
using DiscordGames.Grain.States;
using Microsoft.Extensions.Logging;

namespace DiscordGames.Grain.Implements.GameSessions;

using PlayerNode = LinkedListNode<PerudoSessionGrain.PlayerInfo>;

public class PerudoSessionGrain : Grain<PerudoSessionState>, IPerudoSessionGrain
{
    public const int DefaultMinPlayer = 2;
    public const int DefaultMaxPlayer = 4;
    
    private readonly ILogger<PerudoSessionGrain> logger;

    private PlayerNode? currentTurnNode;

    public PerudoSessionGrain(ILogger<PerudoSessionGrain> logger)
    {
        this.logger = logger;
    }

    public Task<PerudoSessionState> GetState() => Task.FromResult(this.State);

    protected override async Task ReadStateAsync()
    {
        await base.ReadStateAsync();

        if (this.currentTurnNode == null && this.State.CurrentTurn.HasValue)
        {
            var currentTurnUserId = this.State.CurrentTurn.Value;
            var node = this.State.TurnOrder.First!;
            while (node.Next != null && node.Value.UserId != currentTurnUserId)
            {
                node = node.Next;
            }

            this.currentTurnNode = node ?? throw new InvalidOperationException("그 사이에 해당 유저가 사라지면 이상함");
        }
    }

    protected override async Task WriteStateAsync()
    {
        this.State.CurrentTurn = this.State.IsPlaying
            ? this.currentTurnNode?.Value.UserId
            : null;

        await base.WriteStateAsync();
    }

    public Task<InitSessionResult> InitSession(UserId userId, int maxPlayer, bool isClassicRule)
    {
        using var logScope = this.logger.BeginScope("Perudo/InitSession");
        if (this.State.IsPlaying) return Task.FromResult(InitSessionResult.AlreadyStarted);
        if (this.State.IsInitialized) return Task.FromResult(InitSessionResult.AlreadyInitialized);

        this.State.HostUserId = userId;
        this.State.MaxPlayer = maxPlayer;
        this.State.IsClassicRule = isClassicRule;
        this.State.IsInitialized = true;
        
        this.logger.LogPerudoInitSessionOk(this.GetPrimaryKey(), userId, maxPlayer, isClassicRule);
        
        return Task.FromResult(InitSessionResult.Ok);
    }
    
    public Task<JoinPlayerResult> JoinPlayer(UserId userId)
    {
        using var logScope = this.logger.BeginScope("Perudo/JoinPlayer");
        if (this.State.IsPlaying) return Task.FromResult(JoinPlayerResult.AlreadyStarted);
        if (this.State.Players.Contains(userId)) return Task.FromResult(JoinPlayerResult.AlreadyJoined);
        if (this.State.MaxPlayer <= this.State.Players.Count) return Task.FromResult(JoinPlayerResult.MaxPlayer);
        
        // 관전자가 게임에 참가한다면 관전자 목록에서 제거해줍니다
        this.State.Spectators.Remove(userId);
        this.State.Players.Add(userId);
        
        this.logger.LogJoinPlayerOk(this.GetPrimaryKey(), userId);
        
        return Task.FromResult(JoinPlayerResult.Ok);
    }

    public async Task<LeavePlayerResult> LeavePlayer(UserId userId)
    {
        using var logScope = this.logger.BeginScope("Perudo/LeavePlayer");
        if (!this.State.Players.Contains(userId)) return LeavePlayerResult.NotJoinedUser;

        this.State.Players.Remove(userId);
        
        // 플레이 중일 때의 플레이어 이탈 처리
        if (this.State.IsPlaying)
        {
            var node = this.State.TurnOrder.First;
            while (node?.Next != null && node.Value.UserId != userId)
            {
                node = node.Next;
            }

            // 이게 가능한 상황이 아님
            if (node == null) throw new InvalidOperationException();

            this.State.TurnOrder.Remove(node);
            
            //TODO: 이탈 유저 탈락 처리
            await this.StartRound(true);
        }
        
        this.logger.LogLeavePlayerOk(this.GetPrimaryKey(), userId);
        
        return LeavePlayerResult.Ok;
    }

    public Task<JoinSpectatorResult> JoinSpectator(UserId userId)
    {
        using var logScope = this.logger.BeginScope("Perudo/JoinSpectator");
        if (this.State.Players.Contains(userId)) return Task.FromResult(JoinSpectatorResult.AlreadyJoined);
        if (!this.State.Spectators.Add(userId)) return Task.FromResult(JoinSpectatorResult.AlreadyJoined);
        
        this.logger.LogJoinSpectatorOk(this.GetPrimaryKey(), userId);

        return Task.FromResult(JoinSpectatorResult.Ok);
    }

    public Task<LeaveSpectatorResult> LeaveSpectator(UserId userId)
    {
        using var logScope = this.logger.BeginScope("Perudo/LeaveSpectator");
        if (!this.State.Spectators.Remove(userId)) return Task.FromResult(LeaveSpectatorResult.NotJoinedUser);
        
        this.logger.LogLeaveSpectatorOk(this.GetPrimaryKey(), userId);
        
        return Task.FromResult(LeaveSpectatorResult.Ok);
    }

    public async Task<StartGameResult> StartGame(UserId userId)
    {
        using var logScope = this.logger.BeginScope("Perudo/StartGame");
        if (this.State.IsPlaying) return StartGameResult.AlreadyStarted;
        if (this.State.HostUserId != userId) return StartGameResult.NotFromHostUser;
        if (this.State.Players.Count < DefaultMinPlayer) return StartGameResult.MinPlayer;

        this.State.IsPlaying = true;
        
        var shuffledPlayers = this.State.Players
            .OrderBy(_ => Random.Shared.Next())
            .ToArray();

        foreach (var playerUserId in shuffledPlayers)
        {
            this.State.TurnOrder.AddLast(new PlayerInfo(playerUserId, this.State.IsClassicRule ? 5 : 3));
        }

        await this.StartRound();
        
        this.logger.LogStartGameOk(this.GetPrimaryKey(), userId);
        
        return StartGameResult.Ok;
    }

    public Task<PlaceBidResult> PlaceBid(UserId userId, int quantity, int face)
    {
        throw new NotImplementedException();
    }

    private Task StartRound(bool pickRandomFirstPlayer = false)
    {
        using var logScope = this.logger.BeginScope("Perudo/StartGame");
        var alivePlayers = pickRandomFirstPlayer ? new List<PlayerNode>() : null;

        var node = this.State.TurnOrder.First!;
        while (node.Next != null)
        {
            if (!node.Value.IsAlive) continue;
            
            node.Value.Roll();
            alivePlayers!.Add(node);
            
            node = node.Next;
        }

        if (pickRandomFirstPlayer)
        {
            this.currentTurnNode = alivePlayers![Random.Shared.Next(0, alivePlayers.Count)];
        }
        
        this.logger.LogPerudoStartRound(this.GetPrimaryKey(), this.currentTurnNode!.Value.UserId);

        return Task.CompletedTask;
    }

    public class PlayerInfo
    {
        public UserId UserId { get; }
        public int[] Dices { get; }
        public int Life { get; set; }

        public bool IsAlive => 0 < this.Life;

        public PlayerInfo(UserId userId, int initLife)
        {
            this.UserId = userId;
            this.Dices = new int[5];
            this.Life = initLife;
        }

        [JsonConstructor]
        public PlayerInfo(UserId userId, int[] dices, int life)
        {
            this.UserId = userId;
            this.Dices = dices;
            this.Life = life;
        }

        public void Roll()
        {
            for (int i = 0, len = this.Dices.Length; i < len; i++)
            {
                this.Dices[i] = Random.Shared.Next(1, 7);
            }
        }
    }
}

public static partial class Log
{
    [LoggerMessage(LogLevel.Information,
        Message = "Session {session} initialized by {userId} [maxPlayer : {maxPlayer}, isClassicRule : {isClassicRule}]")]
    public static partial void LogPerudoInitSessionOk(this ILogger logger, Guid session, UserId userId, int maxPlayer, bool isClassicRule);
    
    [LoggerMessage(LogLevel.Information,
        Message = "New round started from {session} [firstPlayer : {firstUserId}]")]
    public static partial void LogPerudoStartRound(this ILogger logger, Guid session, UserId firstUserId);
    
    [LoggerMessage(LogLevel.Information,
        Message = "Player {userId} placed bid from {session} [lastBid : ({lastQuantity}, {lastFace}), newBid : ({quantity}, {face})]")]
    public static partial void LogPerudoPlaceBidOk(this ILogger logger, Guid session, UserId userId, int lastQuantity, int lastFace, int quantity, int face);
}