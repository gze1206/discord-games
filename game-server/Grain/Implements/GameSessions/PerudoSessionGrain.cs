using System.Text.Json.Serialization;
using Cathei.LinqGen;
using DiscordGames.Core;
using DiscordGames.Grains.Interfaces;
using DiscordGames.Grains.Interfaces.GameSessions;
using DiscordGames.Grains.LogMessages.GameSession;
using DiscordGames.Grains.LogMessages.Perudo;
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
    
    private void SetSessionInfo(string sessionName, int maxPlayer, bool isClassicRule)
    {
        this.State.SessionName = sessionName;
        this.State.MaxPlayer = maxPlayer;
        this.State.IsClassicRule = isClassicRule;

        this.playerInfoSelector = new PlayerInfoSelector(isClassicRule ? 5 : 3);
    }

    private void MigrateHost()
    {
        var prevHost = this.State.HostUserId;
        var newHost = this.State.Players.FirstOrDefault(defaultValue: -1);
        this.State.HostUserId = newHost;
        
        this.logger.LogMigrateHostOk(prevHost, newHost, this.State.SessionName!, this.GetPrimaryKeyString());
    }

    private async PooledValueTask BroadcastExceptSender(UserId sender, byte[] data)
    {
        foreach (var player in this.State.Players)
        {
            if (player == sender) continue;

            var user = this.GrainFactory.GetGrain<IUserGrain>(player);
            await user.ReserveSend(data);
        }

        foreach (var spectator in this.State.Spectators)
        {
            if (spectator == sender) continue;

            var user = this.GrainFactory.GetGrain<IUserGrain>(spectator);
            await user.ReserveSend(data);
        }
    }

    private async PooledValueTask BroadcastToAll(byte[] data)
    {
        foreach (var player in this.State.Players)
        {
            var user = this.GrainFactory.GetGrain<IUserGrain>(player);
            await user.ReserveSend(data);
        }

        foreach (var spectator in this.State.Spectators)
        {
            var user = this.GrainFactory.GetGrain<IUserGrain>(spectator);
            await user.ReserveSend(data);
        }
    }

    public ValueTask<ResultCode> InitSession(UserId userId, string sessionName, int maxPlayer, bool isClassicRule)
    {
        return Internal(this, userId, sessionName, maxPlayer, isClassicRule);
        static async PooledValueTask<ResultCode> Internal(PerudoSessionGrain self, UserId userId,
            string sessionName, int maxPlayer, bool isClassicRule)
        {
            using (self.logger.BeginScope("Perudo/InitSession"))
            {
                if (self.State.IsPlaying) return ResultCode.AlreadyStarted;
                if (self.State.IsInitialized) return ResultCode.AlreadyInitialized;
                if (maxPlayer is < DefaultMinPlayer or > DefaultMaxPlayer) return ResultCode.InvalidMaxPlayer;

                self.SetSessionInfo(sessionName, maxPlayer, isClassicRule);
                self.State.HostUserId = userId;
                self.State.IsInitialized = true;
                self.State.Players.Add(userId);
            
                var user = self.GrainFactory.GetGrain<IUserGrain>(userId);
                await user.SetSessionUid(self.GetPrimaryKeyString());
            
                self.logger.LogPerudoInitSessionOk(self.GetPrimaryKeyString(), userId, sessionName, maxPlayer, isClassicRule);
            
                return ResultCode.Ok;
            }
        }
    }

    public ValueTask<ResultCode> EditSession(UserId userId, string sessionName, int maxPlayer, bool isClassicRule)
    {
        using (this.logger.BeginScope("Perudo/EditSession"))
        {
            if (this.State.IsPlaying) return ValueTask.FromResult(ResultCode.AlreadyStarted);
            if (!this.State.IsInitialized) return ValueTask.FromResult(ResultCode.NotInitializedGame);
            if (this.State.HostUserId != userId) return ValueTask.FromResult(ResultCode.NotFromHostUser);
            if (maxPlayer is < DefaultMinPlayer or > DefaultMaxPlayer) return ValueTask.FromResult(ResultCode.InvalidMaxPlayer);
            
            this.SetSessionInfo(sessionName, maxPlayer, isClassicRule);
            
            this.logger.LogPerudoEditSessionOk(this.GetPrimaryKeyString(), userId, sessionName, maxPlayer, isClassicRule);

            return ValueTask.FromResult(ResultCode.Ok);
        }
    }

    public ValueTask<ResultCode> JoinPlayer(UserId userId)
    {
        return Internal(this, userId);
        static async PooledValueTask<ResultCode> Internal(PerudoSessionGrain self, UserId userId)
        {
            using (self.logger.BeginScope("Perudo/JoinPlayer"))
            {
                if (self.State.IsPlaying) return ResultCode.AlreadyStarted;
                if (self.State.Players.Contains(userId)) return ResultCode.AlreadyJoined;
                if (self.State.MaxPlayer <= self.State.Players.Count) return ResultCode.ExceedMaxPlayerLimit;
                
                // 관전자가 게임에 참가한다면 관전자 목록에서 제거해줍니다
                self.State.Spectators.Remove(userId);
                self.State.Players.Add(userId);

                var user = self.GrainFactory.GetGrain<IUserGrain>(userId);
                await user.SetSessionUid(self.GetPrimaryKeyString());
                
                self.logger.LogJoinPlayerOk(self.GetPrimaryKeyString(), userId);

                return ResultCode.Ok;
            }
        }
    }

    public ValueTask<ResultCode> LeavePlayer(UserId userId)
    {
        return Internal(this, userId);
        static async PooledValueTask<ResultCode> Internal(PerudoSessionGrain self, UserId userId)
        {
            using (self.logger.BeginScope("Perudo/LeavePlayer"))
            {
                if (!self.State.Players.Contains(userId)) return ResultCode.NotJoinedUser;

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

                var user = self.GrainFactory.GetGrain<IUserGrain>(userId);
                await user.SetSessionUid(null);

                if (userId == self.State.HostUserId) self.MigrateHost();

                self.logger.LogLeavePlayerOk(self.GetPrimaryKeyString(), userId);
                
                return ResultCode.Ok;
            }
        }
    }

    public ValueTask<ResultCode> JoinSpectator(UserId userId)
    {
        return Internal(this, userId);
        static async PooledValueTask<ResultCode> Internal(PerudoSessionGrain self, UserId userId)
        {
            using (self.logger.BeginScope("Perudo/JoinSpectator"))
            {
                if (self.State.Spectators.Contains(userId)) return ResultCode.AlreadyJoined;
                
                // 현재 플레이어로 참가 중인 유저여도 게임 시작 전이라면 관전자로 전환할 수 있습니다
                // 하지만, 게임 시작 이후라면 게임에서 이탈한 다음에 관전자로 참가해야 합니다
                if (self.State.Players.Contains(userId))
                {
                    if (self.State.IsPlaying) return ResultCode.AlreadyJoined;

                    self.State.Players.Remove(userId);
                }
                
                var user = self.GrainFactory.GetGrain<IUserGrain>(userId);
                await user.SetSessionUid(self.GetPrimaryKeyString());

                self.State.Spectators.Add(userId);
                
                self.logger.LogJoinSpectatorOk(self.GetPrimaryKeyString(), userId);

                return ResultCode.Ok;
            }
        }
    }

    public ValueTask<ResultCode> LeaveSpectator(UserId userId)
    {
        return Internal(this, userId);
        static async PooledValueTask<ResultCode> Internal(PerudoSessionGrain self, UserId userId)
        {
            using (self.logger.BeginScope("Perudo/LeaveSpectator"))
            {
                if (!self.State.Spectators.Remove(userId)) return ResultCode.NotJoinedUser;
            
                var user = self.GrainFactory.GetGrain<IUserGrain>(userId);
                await user.SetSessionUid(null);
                
                if (userId == self.State.HostUserId) self.MigrateHost();
            
                self.logger.LogLeaveSpectatorOk(self.GetPrimaryKeyString(), userId);
            
                return ResultCode.Ok;
            }
        }
    }

    public ValueTask<ResultCode> LeaveUser(UserId userId)
    {
        return Internal(this, userId);
        static async PooledValueTask<ResultCode> Internal(PerudoSessionGrain self, UserId userId)
        {
            using (self.logger.BeginScope("Perudo/LeaveUser"))
            {
                if (self.State.Spectators.Contains(userId))
                {
                    return await self.LeaveSpectator(userId);
                } 
                else
                {
                    return await self.LeavePlayer(userId);
                }
            }
        }
    }

    public ValueTask<ResultCode> StartGame(UserId userId)
    {
        return Internal(this, userId);
        
        static async PooledValueTask<ResultCode> Internal(PerudoSessionGrain self, UserId userId)
        {
            using (self.logger.BeginScope("Perudo/StartGame"))
            {
                if (self.State.IsPlaying) return ResultCode.AlreadyStarted;
                if (self.State.HostUserId != userId) return ResultCode.NotFromHostUser;
                if (self.State.Players.Count < DefaultMinPlayer) return ResultCode.ExceedMinPlayerLimit;
                
                self.State.TurnOrder.AddRange(self.State.Players
                    .Gen()
                    .OrderBy(_ => Random.Shared.Next())
                    .Select(self.playerInfoSelector)
                    .AsEnumerable());

                self.State.IsPlaying = true;
                await self.StartRound(true);
                
                self.logger.LogStartGameOk(self.GetPrimaryKeyString(), userId);
                
                return ResultCode.Ok;
            }
        }
    }

    public ValueTask<ResultCode> PlaceBid(UserId userId, int quantity, int face)
    {
        using (this.logger.BeginScope("Perudo/PlaceBid"))
        {
            if (!this.State.IsPlaying) return ValueTask.FromResult(ResultCode.NotStartedGame);
            if (this.currentTurnInfo?.UserId != userId) return ValueTask.FromResult(ResultCode.NotFromCurrentTurnUser);
            if (quantity < 1 || this.totalDices < quantity) return ValueTask.FromResult(ResultCode.InvalidQuantity);
            if (face is < 1 or > 6) return ValueTask.FromResult(ResultCode.InvalidFace);

            if (!this.currentTurnInfo.IsAlive) GrainThrowHelper.ThrowInvalidTurnPlaying();
            
            var lastQuantity = this.State.LastBidQuantity;
            var lastFace = this.State.LastBidFace;

            if (this.State.IsClassicRule)
            {
                // Classic : 주사위의 수가 더 높거나, 같은 수의 더 높은 눈을 제시해야 합니다
                if (quantity < lastQuantity)
                {
                    return ValueTask.FromResult(ResultCode.CannotLowerQuantityBid);
                }
                if (quantity == lastQuantity && face <= lastFace)
                {
                    return ValueTask.FromResult(ResultCode.CannotLowerFaceBid);
                }
            }
            else
            {
                // Simple : 주사위의 수가 더 높아져야 합니다
                if (quantity <= lastQuantity) return ValueTask.FromResult(ResultCode.CannotLowerQuantityBid);
            }

            this.State.LastBidUserId = userId;
            this.State.LastBidQuantity = quantity;
            this.State.LastBidFace = face;
            this.State.CurrentTurn = (this.State.CurrentTurn + 1) % this.State.TurnOrder.Count;
            this.lastBidderInfo = this.currentTurnInfo;
            this.currentTurnInfo = this.State.TurnOrder[this.State.CurrentTurn];

            this.logger.LogPerudoPlaceBidOk(this.GetPrimaryKeyString(), userId, lastQuantity, lastFace, quantity, face);
            
            return ValueTask.FromResult(ResultCode.Ok);
        }
    }

    public ValueTask<ResultCode> Challenge(UserId userId)
    {
        return Internal(this, userId);
        
        static async PooledValueTask<ResultCode> Internal(PerudoSessionGrain self, UserId userId)
        {
            using (self.logger.BeginScope("Perudo/Challenge"))
            {
                var lastBidder = self.State.LastBidUserId;
                
                if (!self.State.IsPlaying) return ResultCode.NotStartedGame;
                if (lastBidder < 0 || self.lastBidderInfo == null) return ResultCode.NoPreviousBid;
                if (self.currentTurnInfo?.UserId != userId) return ResultCode.NotFromCurrentTurnUser;

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

                self.logger.LogPerudoChallengeOk(self.GetPrimaryKeyString(), userId, lastBidder, lastBidQuantity, face, actualQuantity);
                
                return ResultCode.Ok;
            }
        }
    }

    private async ValueTask ProcessDropout(PlayerInfo dropout)
    {
        this.State.TurnOrder.Remove(dropout);
        await this.StartRound(pickRandomFirstPlayer: true);
    }

    private ValueTask StartRound(bool pickRandomFirstPlayer = false)
    {
        using (this.logger.BeginScope("Perudo/StartGame"))
        {
            if (pickRandomFirstPlayer)
            {
                this.State.CurrentTurn = Random.Shared.Next(0, this.State.TurnOrder.Count);
                this.currentTurnInfo = this.State.TurnOrder[this.State.CurrentTurn];
            }

            this.State.LastBidUserId = -1;
            this.State.LastBidQuantity = -1;
            this.State.LastBidFace = -1;
            this.lastBidderInfo = null;
            
            this.logger.LogPerudoStartRound(this.GetPrimaryKeyString(), this.currentTurnInfo?.UserId ?? throw new InvalidOperationException("생존한 플레이어가 없습니다"));

            return ValueTask.CompletedTask;
        }
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
