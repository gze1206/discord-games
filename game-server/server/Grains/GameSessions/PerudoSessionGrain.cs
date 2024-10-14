using server.Core;

namespace server.Grains.GameSessions;

using PlayerNode = LinkedListNode<PerudoSessionGrain.PlayerInfo>;

public class PerudoSessionGrain : Grain, IPerudoSessionGrain
{
    public const int DefaultMinPlayer = 2;
    public const int DefaultMaxPlayer = 4;
    
    private readonly HashSet<UserId> players = new();
    private readonly HashSet<UserId> spectators = new();
    private readonly LinkedList<PlayerInfo> turnOrder = new();

    private UserId hostUserId;
    private bool isPlaying = false;
    private bool isClassicRule = false;
    private int maxPlayer = DefaultMaxPlayer;
    private PlayerNode? currentTurn;
    
    public Task<JoinPlayerResult> JoinPlayer(UserId userId)
    {
        if (this.isPlaying) return Task.FromResult(JoinPlayerResult.AlreadyStarted);
        if (this.players.Contains(userId)) return Task.FromResult(JoinPlayerResult.AlreadyJoined);
        if (this.maxPlayer <= this.players.Count) return Task.FromResult(JoinPlayerResult.MaxPlayer);
        
        // 관전자가 게임에 참가한다면 관전자 목록에서 제거해줍니다
        this.spectators.Remove(userId);
        this.players.Add(userId);
            
        return Task.FromResult(JoinPlayerResult.Ok);
    }

    public async Task<LeavePlayerResult> LeavePlayer(UserId userId)
    {
        if (!this.players.Contains(userId)) return LeavePlayerResult.NotJoinedUser;

        this.players.Remove(userId);
        
        // 플레이 중일 때의 플레이어 이탈 처리
        if (this.isPlaying)
        {
            var node = this.turnOrder.First;
            while (node?.Next != null && node.Value.UserId != userId)
            {
                node = node.Next;
            }

            // 이게 가능한 상황이 아님
            if (node == null) throw new InvalidOperationException();

            this.turnOrder.Remove(node);
            
            //TODO: 이탈 유저 탈락 처리
            await this.StartRound(true);
        }
        
        return LeavePlayerResult.Ok;
    }

    public Task<JoinSpectatorResult> JoinSpectator(UserId userId)
    {
        if (this.players.Contains(userId)) return Task.FromResult(JoinSpectatorResult.AlreadyJoined);

        return Task.FromResult(this.spectators.Add(userId)
            ? JoinSpectatorResult.Ok
            : JoinSpectatorResult.AlreadyJoined);
    }

    public Task<LeaveSpectatorResult> LeaveSpectator(UserId userId)
    {
        return Task.FromResult(this.spectators.Remove(userId)
            ? LeaveSpectatorResult.Ok
            : LeaveSpectatorResult.NotJoinedUser);
    }

    public async Task<StartGameResult> StartGame(UserId userId)
    {
        if (this.isPlaying) return StartGameResult.AlreadyStarted;
        if (this.hostUserId != userId) return StartGameResult.NotFromHostUser;
        if (this.players.Count < DefaultMinPlayer) return StartGameResult.MinPlayer;

        this.isPlaying = true;
        
        var shuffledPlayers = this.players
            .OrderBy(_ => Random.Shared.Next())
            .ToArray();

        foreach (var playerUserId in shuffledPlayers)
        {
            this.turnOrder.AddLast(new PlayerInfo(playerUserId, this.isClassicRule ? 5 : 3));
        }

        await this.StartRound();
        
        return StartGameResult.Ok;
    }

    public Task<PlaceBidResult> PlaceBid(UserId userId, int quantity, int face)
    {
        throw new NotImplementedException();
    }

    private Task StartRound(bool pickRandomFirstPlayer = false)
    {
        var alivePlayers = pickRandomFirstPlayer ? new List<PlayerNode>() : null;

        var node = this.turnOrder.First!;
        while (node.Next != null)
        {
            if (!node.Value.IsAlive) continue;
            
            node.Value.Roll();
            alivePlayers!.Add(node);
            
            node = node.Next;
        }

        if (pickRandomFirstPlayer)
        {
            this.currentTurn = alivePlayers![Random.Shared.Next(0, alivePlayers.Count)];
        }

        return Task.CompletedTask;
    }

    public class PlayerInfo
    {
        public UserId UserId { get; init; }
        public int[] Dices { get; private set; }
        public int Life { get; set; }

        public bool IsAlive => 0 < this.Life;

        public PlayerInfo(UserId userId, int initLife)
        {
            this.UserId = userId;
            this.Dices = new int[5];
            this.Life = initLife;
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