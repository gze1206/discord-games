using System.Text.Json.Serialization;
using Cathei.LinqGen;
using DiscordGames.Grains.Interfaces.GameSessions;
using DiscordGames.Grains.ResultCodes.CommonSession;
using DiscordGames.Grains.ResultCodes.PerudoSession;
using DiscordGames.Grains.States;
using Microsoft.Extensions.Logging;
using PooledAwait;

namespace DiscordGames.Grains.Implements.GameSessions;

public class PerudoSessionGrain : Grain<PerudoSessionState>, IPerudoSessionGrain
{
    public const int DefaultMinPlayer = 2;
    public const int DefaultMaxPlayer = 4;
    
    private readonly ILogger<PerudoSessionGrain> logger;

    private PlayerInfo? currentTurnInfo;
    private PlayerInfo? lastBidderInfo;
    private PlayerInfoSelector playerInfoSelector;
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
                GrainThrowHelper.ThrowInvalidTurnOrderState();
            
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

    public ValueTask<InitPerudoSessionResult> InitSession(UserId userId, string sessionName, int maxPlayer, bool isClassicRule)
    {
        using var logScope = this.logger.BeginScope("Perudo/InitSession");
        if (this.State.IsPlaying) return ValueTask.FromResult(InitPerudoSessionResult.AlreadyStarted);
        if (this.State.IsInitialized) return ValueTask.FromResult(InitPerudoSessionResult.AlreadyInitialized);
        if (maxPlayer is < DefaultMinPlayer or > DefaultMaxPlayer) return ValueTask.FromResult(InitPerudoSessionResult.InvalidMaxPlayer);

        this.State.HostUserId = userId;
        this.State.SessionName = sessionName;
        this.State.MaxPlayer = maxPlayer;
        this.State.IsClassicRule = isClassicRule;
        this.State.IsInitialized = true;
        this.State.Players.Add(userId);

        this.playerInfoSelector = new PlayerInfoSelector(isClassicRule ? 5 : 3);
        
        this.logger.LogPerudoInitSessionOk(this.GetPrimaryKey(), userId, sessionName, maxPlayer, isClassicRule);
        
        return ValueTask.FromResult(InitPerudoSessionResult.Ok);
    }
    
    public ValueTask<JoinPlayerResult> JoinPlayer(UserId userId)
    {
        using var logScope = this.logger.BeginScope("Perudo/JoinPlayer");
        if (this.State.IsPlaying) return ValueTask.FromResult(JoinPlayerResult.AlreadyStarted);
        if (this.State.Players.Contains(userId)) return ValueTask.FromResult(JoinPlayerResult.AlreadyJoined);
        if (this.State.MaxPlayer <= this.State.Players.Count) return ValueTask.FromResult(JoinPlayerResult.MaxPlayer);
        
        // 관전자가 게임에 참가한다면 관전자 목록에서 제거해줍니다
        this.State.Spectators.Remove(userId);
        this.State.Players.Add(userId);
        
        this.logger.LogJoinPlayerOk(this.GetPrimaryKey(), userId);

        return ValueTask.FromResult(JoinPlayerResult.Ok);
    }

    public ValueTask<LeavePlayerResult> LeavePlayer(UserId userId)
    {
        return Internal(this, userId);
        static async PooledValueTask<LeavePlayerResult> Internal(PerudoSessionGrain self, UserId userId)
        {
            using var logScope = self.logger.BeginScope("Perudo/LeavePlayer");
            if (!self.State.Players.Contains(userId)) return LeavePlayerResult.NotJoinedUser;

            self.State.Players.Remove(userId);
            
            // 플레이 중일 때의 플레이어 이탈 처리
            if (self.State.IsPlaying)
            {
                var userInfo = self.State.TurnOrder.Find(x => x.UserId == userId);

                if (userInfo != null)
                {
                    await self.ProcessDropout(userInfo);
                }
            }
            
            self.logger.LogLeavePlayerOk(self.GetPrimaryKey(), userId);
            
            return LeavePlayerResult.Ok;
        }
    }

    public ValueTask<JoinSpectatorResult> JoinSpectator(UserId userId)
    {
        using var logScope = this.logger.BeginScope("Perudo/JoinSpectator");
        if (this.State.Spectators.Contains(userId)) return ValueTask.FromResult(JoinSpectatorResult.AlreadyJoined);
        
        // 현재 플레이어로 참가 중인 유저여도 게임 시작 전이라면 관전자로 전환할 수 있습니다
        // 하지만, 게임 시작 이후라면 게임에서 이탈한 다음에 관전자로 참가해야 합니다
        if (this.State.Players.Contains(userId))
        {
            if (this.State.IsPlaying) return ValueTask.FromResult(JoinSpectatorResult.AlreadyJoined);

            this.State.Players.Remove(userId);
        }

        this.State.Spectators.Add(userId);
        
        this.logger.LogJoinSpectatorOk(this.GetPrimaryKey(), userId);

        return ValueTask.FromResult(JoinSpectatorResult.Ok);
    }

    public ValueTask<LeaveSpectatorResult> LeaveSpectator(UserId userId)
    {
        using var logScope = this.logger.BeginScope("Perudo/LeaveSpectator");
        if (!this.State.Spectators.Remove(userId)) return ValueTask.FromResult(LeaveSpectatorResult.NotJoinedUser);
        
        this.logger.LogLeaveSpectatorOk(this.GetPrimaryKey(), userId);
        
        return ValueTask.FromResult(LeaveSpectatorResult.Ok);
    }

    public ValueTask<StartGameResult> StartGame(UserId userId)
    {
        return Internal(this, userId);
        
        static async PooledValueTask<StartGameResult> Internal(PerudoSessionGrain self, UserId userId)
        {
            using var logScope = self.logger.BeginScope("Perudo/StartGame");
            if (self.State.IsPlaying) return StartGameResult.AlreadyStarted;
            if (self.State.HostUserId != userId) return StartGameResult.NotFromHostUser;
            if (self.State.Players.Count < DefaultMinPlayer) return StartGameResult.MinPlayer;
            
            self.State.TurnOrder.AddRange(self.State.Players
                .Gen()
                .OrderBy(_ => Random.Shared.Next())
                .Select(self.playerInfoSelector)
                .AsEnumerable());

            self.State.IsPlaying = true;
            await self.StartRound(true);
            
            self.logger.LogStartGameOk(self.GetPrimaryKey(), userId);
            
            return StartGameResult.Ok;
        }
    }

    public ValueTask<PlaceBidResult> PlaceBid(UserId userId, int quantity, int face)
    {
        using var logScope = this.logger.BeginScope("Perudo/PlaceBid");
        if (!this.State.IsPlaying) return ValueTask.FromResult(PlaceBidResult.NotStartedGame);
        if (this.currentTurnInfo?.UserId != userId) return ValueTask.FromResult(PlaceBidResult.NotFromCurrentTurnUser);
        if (quantity < 1 || this.totalDices < quantity) return ValueTask.FromResult(PlaceBidResult.InvalidQuantity);
        if (face is < 1 or > 6) return ValueTask.FromResult(PlaceBidResult.InvalidFace);

        if (!this.currentTurnInfo.IsAlive) GrainThrowHelper.ThrowInvalidTurnPlaying();
        
        var lastQuantity = this.State.LastBidQuantity;
        var lastFace = this.State.LastBidFace;

        if (this.State.IsClassicRule)
        {
            // Classic : 주사위의 수가 더 높거나, 같은 수의 더 높은 눈을 제시해야 합니다
            if (quantity < lastQuantity)
            {
                return ValueTask.FromResult(PlaceBidResult.CannotLowerQuantityBid);
            }
            if (quantity == lastQuantity && face <= lastFace)
            {
                return ValueTask.FromResult(PlaceBidResult.CannotLowerFaceBid);
            }
        }
        else
        {
            // Simple : 주사위의 수가 더 높아져야 합니다
            if (quantity <= lastQuantity) return ValueTask.FromResult(PlaceBidResult.CannotLowerQuantityBid);
        }

        this.State.LastBidUserId = userId;
        this.State.LastBidQuantity = quantity;
        this.State.LastBidFace = face;
        this.State.CurrentTurn = (this.State.CurrentTurn + 1) % this.State.TurnOrder.Count;
        this.lastBidderInfo = this.currentTurnInfo;
        this.currentTurnInfo = this.State.TurnOrder[this.State.CurrentTurn];

        this.logger.LogPerudoPlaceBidOk(this.GetPrimaryKey(), userId, lastQuantity, lastFace, quantity, face);
        
        return ValueTask.FromResult(PlaceBidResult.Ok);
    }

    public ValueTask<ChallengeResult> Challenge(UserId userId)
    {
        return Internal(this, userId);
        
        static async PooledValueTask<ChallengeResult> Internal(PerudoSessionGrain self, UserId userId)
        {
            var lastBidder = self.State.LastBidUserId;
            using var logScope = self.logger.BeginScope("Perudo/Challenge");
            if (!self.State.IsPlaying) return ChallengeResult.NotStartedGame;
            if (lastBidder < 0 || self.lastBidderInfo == null) return ChallengeResult.NoPreviousBid;
            if (self.currentTurnInfo?.UserId != userId) return ChallengeResult.NotFromCurrentTurnUser;

            if (!self.currentTurnInfo.IsAlive) GrainThrowHelper.ThrowInvalidTurnPlaying();

            var lastBidQuantity = self.State.LastBidQuantity;
            var face = self.State.LastBidFace;
            var faceCounter = new EqualFunc<int>(face);

            var actualQuantity = 0;
            foreach (var dice in self.State.TurnOrder
                         .Gen()
                         .Select(player => player.Dices))
            {
                actualQuantity += dice.Gen().Count(faceCounter);
            }

            // 선언이 유효하지 않았으면 선언자, 선언이 유효했으면 도전자의 패배입니다
            var loser = lastBidQuantity < actualQuantity
                ? self.lastBidderInfo!
                : self.currentTurnInfo!;

            loser.Life--;
            if (self.State.IsClassicRule) loser.Dices.RemoveLast();

            if (!loser.IsAlive)
            {
                await self.ProcessDropout(loser);
            }
            else
            {
                self.State.CurrentTurn = self.State.TurnOrder.IndexOf(loser);
                self.currentTurnInfo = loser;
                await self.StartRound();
            }

            self.logger.LogPerudoChallengeOk(self.GetPrimaryKey(), userId, lastBidder, lastBidQuantity, face, actualQuantity);
            
            return ChallengeResult.Ok;
        }
    }

    private async ValueTask ProcessDropout(PlayerInfo dropout)
    {
        this.State.TurnOrder.Remove(dropout);
        await this.StartRound(pickRandomFirstPlayer: true);
    }

    private ValueTask StartRound(bool pickRandomFirstPlayer = false)
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

        return ValueTask.CompletedTask;
    }
    
    [GenerateSerializer]
    [Alias("DiscordGames.Grain.Implements.GameSessions.PerudoSessionGrain.PlayerInfo")]
    public class PlayerInfo
    {
        [Id(0)]
        public UserId UserId { get; }
        [Id(1)]
        public LinkedList<int> Dices { get; }
        [Id(2)]
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
    
    private readonly struct PlayerInfoSelector : IStructFunction<int, PlayerInfo>
    {
        private readonly int initLife;

        public PlayerInfoSelector(int initLife)
        {
            this.initLife = initLife;
        }

        public PlayerInfo Invoke(int playerId) => new(playerId, this.initLife);
    }

    private readonly struct EqualFunc<T> : IStructFunction<T, bool>
        where T : IEquatable<T>
    {
        private readonly T value;

        public EqualFunc(T value)
        {
            this.value = value;
        }

        public bool Invoke(T cur) => this.value.Equals(cur);
    }
}

public static partial class Log
{
    [LoggerMessage(LogLevel.Information,
        Message = "Session {sessionName}({session}) initialized by {userId} [maxPlayer : {maxPlayer}, isClassicRule : {isClassicRule}]")]
    public static partial void LogPerudoInitSessionOk(this ILogger logger, Guid session, UserId userId, string sessionName, int maxPlayer, bool isClassicRule);
    
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