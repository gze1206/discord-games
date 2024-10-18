using System.Text.Json.Serialization;
using DiscordGames.Core.ResultCodes.CommonSession;
using DiscordGames.Core.ResultCodes.PerudoSession;
using DiscordGames.Grain.Interfaces.GameSessions;
using DiscordGames.Grain.States;
using Microsoft.Extensions.Logging;

namespace DiscordGames.Grain.Implements.GameSessions;

public class PerudoSessionGrain : Grain<PerudoSessionState>, IPerudoSessionGrain
{
    public const int DefaultMinPlayer = 2;
    public const int DefaultMaxPlayer = 4;
    
    private readonly ILogger<PerudoSessionGrain> logger;

    private PlayerInfo? currentTurnInfo;
    private PlayerInfo? lastBidderInfo;
    private int totalDices;

    public PerudoSessionGrain(ILogger<PerudoSessionGrain> logger)
    {
        this.logger = logger;
    }

    public Task<PerudoSessionState> GetState() => Task.FromResult(this.State);

    protected override async Task ReadStateAsync()
    {
        await base.ReadStateAsync();

        if (this.currentTurnInfo == null && 0 <= this.State.CurrentTurn)
        {
            if (this.State.TurnOrder.Count <= this.State.CurrentTurn)
                throw new InvalidOperationException("State의 현재 턴 정보와 턴 순서 정보가 불일치함");
            
            this.totalDices = 0;
            this.currentTurnInfo = this.State.TurnOrder[this.State.CurrentTurn];

            foreach (var player in this.State.TurnOrder)
            {
                if (!player.IsAlive) continue;

                if (player.UserId == this.State.LastBidUserId)
                {
                    this.lastBidderInfo = player;
                }
                
                this.totalDices += player.Dices.Count;
            }
        }
    }

    public Task<InitPerudoSessionResult> InitSession(UserId userId, int maxPlayer, bool isClassicRule)
    {
        using var logScope = this.logger.BeginScope("Perudo/InitSession");
        if (this.State.IsPlaying) return Task.FromResult(InitPerudoSessionResult.AlreadyStarted);
        if (this.State.IsInitialized) return Task.FromResult(InitPerudoSessionResult.AlreadyInitialized);
        if (maxPlayer is < DefaultMinPlayer or > DefaultMaxPlayer) return Task.FromResult(InitPerudoSessionResult.InvalidMaxPlayer);

        this.State.HostUserId = userId;
        this.State.MaxPlayer = maxPlayer;
        this.State.IsClassicRule = isClassicRule;
        this.State.IsInitialized = true;
        this.State.Players.Add(userId);
        
        this.logger.LogPerudoInitSessionOk(this.GetPrimaryKey(), userId, maxPlayer, isClassicRule);
        
        return Task.FromResult(InitPerudoSessionResult.Ok);
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
            var userInfo = this.State.TurnOrder.Find(x => x.UserId == userId);

            if (userInfo != null)
            {
                await this.ProcessDropout(userInfo);
            }
        }
        
        this.logger.LogLeavePlayerOk(this.GetPrimaryKey(), userId);
        
        return LeavePlayerResult.Ok;
    }

    public Task<JoinSpectatorResult> JoinSpectator(UserId userId)
    {
        using var logScope = this.logger.BeginScope("Perudo/JoinSpectator");
        if (this.State.Spectators.Contains(userId)) return Task.FromResult(JoinSpectatorResult.AlreadyJoined);
        
        // 현재 플레이어로 참가 중인 유저여도 게임 시작 전이라면 관전자로 전환할 수 있습니다
        // 하지만, 게임 시작 이후라면 게임에서 이탈한 다음에 관전자로 참가해야 합니다
        if (this.State.Players.Contains(userId))
        {
            if (this.State.IsPlaying) return Task.FromResult(JoinSpectatorResult.AlreadyJoined);

            this.State.Players.Remove(userId);
        }

        this.State.Spectators.Add(userId);
        
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
        
        var initLife = this.State.IsClassicRule ? 5 : 3;
        this.State.TurnOrder.AddRange(this.State.Players
            .OrderBy(_ => Random.Shared.Next())
            .Select(playerUserId => new PlayerInfo(playerUserId, initLife)));

        this.State.IsPlaying = true;
        await this.StartRound(true);
        
        this.logger.LogStartGameOk(this.GetPrimaryKey(), userId);
        
        return StartGameResult.Ok;
    }

    public Task<PlaceBidResult> PlaceBid(UserId userId, int quantity, int face)
    {
        using var logScope = this.logger.BeginScope("Perudo/PlaceBid");
        if (!this.State.IsPlaying) return Task.FromResult(PlaceBidResult.NotStartedGame);
        if (this.currentTurnInfo?.UserId != userId) return Task.FromResult(PlaceBidResult.NotFromCurrentTurnUser);
        if (quantity < 1 || this.totalDices < quantity) return Task.FromResult(PlaceBidResult.InvalidQuantity);
        if (face is < 1 or > 6) return Task.FromResult(PlaceBidResult.InvalidFace);

        if (!this.currentTurnInfo.IsAlive) throw new InvalidOperationException("탈락자의 턴이 진행 중입니다");
        
        var lastQuantity = this.State.LastBidQuantity;
        var lastFace = this.State.LastBidFace;

        if (this.State.IsClassicRule)
        {
            // Classic : 주사위의 수가 더 높거나, 같은 수의 더 높은 눈을 제시해야 합니다
            if (quantity < lastQuantity)
            {
                return Task.FromResult(PlaceBidResult.CannotLowerQuantityBid);
            }
            if (quantity == lastQuantity && face <= lastFace)
            {
                return Task.FromResult(PlaceBidResult.CannotLowerFaceBid);
            }
        }
        else
        {
            // Simple : 주사위의 수가 더 높아져야 합니다
            if (quantity <= lastQuantity) return Task.FromResult(PlaceBidResult.CannotLowerQuantityBid);
        }

        this.State.LastBidUserId = userId;
        this.State.LastBidQuantity = quantity;
        this.State.LastBidFace = face;
        this.State.CurrentTurn = (this.State.CurrentTurn + 1) % this.State.TurnOrder.Count;
        this.lastBidderInfo = this.currentTurnInfo;
        this.currentTurnInfo = this.State.TurnOrder[this.State.CurrentTurn];

        this.logger.LogPerudoPlaceBidOk(this.GetPrimaryKey(), userId, lastQuantity, lastFace, quantity, face);
        
        return Task.FromResult(PlaceBidResult.Ok);
    }

    public async Task<ChallengeResult> Challenge(UserId userId)
    {
        var lastBidder = this.State.LastBidUserId;
        using var logScope = this.logger.BeginScope("Perudo/Challenge");
        if (!this.State.IsPlaying) return ChallengeResult.NotStartedGame;
        if (lastBidder < 0 || this.lastBidderInfo == null) return ChallengeResult.NoPreviousBid;
        if (this.currentTurnInfo?.UserId != userId) return ChallengeResult.NotFromCurrentTurnUser;

        if (!this.currentTurnInfo.IsAlive) throw new InvalidOperationException("탈락자의 턴이 진행 중입니다");

        var lastBidQuantity = this.State.LastBidQuantity;
        var face = this.State.LastBidFace;

        var actualQuantity = 0;
        foreach (var player in this.State.TurnOrder)
        {
            foreach (var die in player.Dices)
            {
                if (die != face) continue;
                actualQuantity++;
            }
        }

        // 선언이 유효하지 않았으면 선언자, 선언이 유효했으면 도전자의 패배입니다
        var loser = lastBidQuantity < actualQuantity
            ? this.lastBidderInfo!
            : this.currentTurnInfo!;

        loser.Life--;
        if (this.State.IsClassicRule) loser.Dices.RemoveLast();

        if (!loser.IsAlive)
        {
            await this.ProcessDropout(loser);
        }
        else
        {
            this.State.CurrentTurn = this.State.TurnOrder.IndexOf(loser);
            this.currentTurnInfo = loser;
            await this.StartRound();
        }

        this.logger.LogPerudoChallengeOk(this.GetPrimaryKey(), userId, lastBidder, lastBidQuantity, face, actualQuantity);
        
        return ChallengeResult.Ok;
    }

    private async Task ProcessDropout(PlayerInfo dropout)
    {
        this.State.TurnOrder.Remove(dropout);
        await this.StartRound(pickRandomFirstPlayer: true);
    }

    private Task StartRound(bool pickRandomFirstPlayer = false)
    {
        using var logScope = this.logger.BeginScope("Perudo/StartGame");

        if (pickRandomFirstPlayer)
        {
            this.State.CurrentTurn = Random.Shared.Next(0, this.State.TurnOrder.Count);
            this.currentTurnInfo = this.State.TurnOrder[this.State.CurrentTurn];
        }

        this.State.LastBidUserId = -1;
        this.State.LastBidQuantity = -1;
        this.State.LastBidFace = -1;
        this.lastBidderInfo = null;
        
        this.logger.LogPerudoStartRound(this.GetPrimaryKey(), this.currentTurnInfo?.UserId ?? throw new InvalidOperationException("생존한 플레이어가 없습니다"));

        return Task.CompletedTask;
    }

    public class PlayerInfo
    {
        public UserId UserId { get; }
        public LinkedList<int> Dices { get; }
        public int Life { get; set; }

        public bool IsAlive => 0 < this.Life;

        private static readonly int[] InitDices = { 0, 0, 0, 0, 0 };

        public PlayerInfo(UserId userId, int initLife)
        {
            this.UserId = userId;
            this.Dices = new LinkedList<int>(InitDices);
            this.Life = initLife;
        }

        [JsonConstructor]
        public PlayerInfo(UserId userId, LinkedList<int> dices, int life)
        {
            this.UserId = userId;
            this.Dices = dices;
            this.Life = life;
        }

        public void Roll()
        {
            for (var node = this.Dices.First; node?.Next != null; node = node.Next)
            {
                node.ValueRef = Random.Shared.Next(1, 7);
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
    
    [LoggerMessage(LogLevel.Information,
        Message = "Player {userId} challenged to {bidder} from {session} [lastBid : ({lastQuantity}, {lastFace}), actualQuantity : {actualQuantity}]")]
    public static partial void LogPerudoChallengeOk(this ILogger logger, Guid session, UserId userId, UserId bidder, int lastQuantity, int lastFace, int actualQuantity);
}