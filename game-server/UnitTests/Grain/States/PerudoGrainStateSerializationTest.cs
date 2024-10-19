using DiscordGames.Grain.Implements.GameSessions;
using DiscordGames.Grain.States;
using DiscordGames.Server.Serialization.Json;
using UnitTests.Utils;

namespace UnitTests.Grain.States;

[TestClass, TestCategory("Serialization"), TestCategory("Grain")]
public class PerudoGrainStateSerializationTest
{
    private CustomJsonGrainStorageSerializer serializer = default!;

    [TestInitialize]
    public void Init()
    {
        this.serializer = Globals.Serializer();
    }

    private static PerudoSessionState GenState(int value)
    {
        var players = new HashSet<UserId>
        {
            1, 2, 3,
        };

        var state = new PerudoSessionState
        {
            Spectators = { 99 },
            HostUserId = players.First(),
            IsInitialized = true,
            IsPlaying = true,
            IsClassicRule = false,
            CurrentTurn = 0,
            LastBidUserId = players.Count - 1,
            LastBidQuantity = value,
            LastBidFace = 3,
        };

        foreach (var playerId in players)
        {
            state.Players.Add(playerId);

            var info = new PerudoSessionGrain.PlayerInfo(playerId, 3);
            info.Roll();
            state.TurnOrder.Add(info);
        }

        return state;
    }

    [TestMethod]
    public void PerudoSessionState__직렬화_후_다시_역직렬화하여_원본과_비교__일치()
    {
        // Arrange
        var state = GenState(4);

        // Act
        var serialized = this.serializer.Serialize(state);
        var deserialized = this.serializer.Deserialize<PerudoSessionState>(serialized);
        
        // Assert
        Assert.IsNotNull(deserialized, "역직렬화 결과가 NULL이면 안됩니다.");
        MyAssert.AreSequenceEquals(state.Players, deserialized.Players, "역직렬화 이후 플레이어 목록이 바뀌면 안됩니다.");
        MyAssert.AreSequenceEquals(state.Spectators, deserialized.Spectators, "역직렬화 이후 관전자 목록이 바뀌면 안됩니다.");
        MyAssert.AreSequenceEquals(state.Players, deserialized.Players, "역직렬화 이후 플레이어 목록이 바뀌면 안됩니다.");
        Assert.AreEqual(state.HostUserId, deserialized.HostUserId, "역직렬화 이후 호스트 유저가 바뀌면 안됩니다.");
        Assert.AreEqual(state.IsInitialized, deserialized.IsInitialized, "역직렬화 이후 초기화 여부가 바뀌면 안됩니다.");
        Assert.AreEqual(state.IsPlaying, deserialized.IsPlaying, "역직렬화 이후 플레이 상태가 바뀌면 안됩니다.");
        Assert.AreEqual(state.IsClassicRule, deserialized.IsClassicRule, "역직렬화 이후 규칙 설정이 바뀌면 안됩니다.");
        Assert.AreEqual(state.CurrentTurn, deserialized.CurrentTurn, "역직렬화 이후 현재 턴 정보가 바뀌면 안됩니다.");
        Assert.AreEqual(state.LastBidUserId, deserialized.LastBidUserId, "역직렬화 이후 마지막으로 선언한 플레이어의 ID가 바뀌면 안됩니다.");
        Assert.AreEqual(state.LastBidQuantity, deserialized.LastBidQuantity, "역직렬화 이후 마지막으로 선언된 주사위의 수 정보가 바뀌면 안됩니다.");
        Assert.AreEqual(state.LastBidFace, deserialized.LastBidFace, "역직렬화 이후 마지막으로 선언된 주사위의 눈 정보가 바뀌면 안됩니다.");
        
        MyAssert.AreSequenceEquals(state.TurnOrder, deserialized.TurnOrder, (expected, actual) =>
        {
            Assert.AreEqual(expected.UserId, actual.UserId, "역직렬화 이후 턴 순서 정보 중 UserId가 바뀌면 안됩니다.");
            Assert.AreEqual(expected.Life, actual.Life, "역직렬화 이후 턴 순서 정보 중 라이프가 바뀌면 안됩니다.");
            MyAssert.AreSequenceEquals(expected.Dices, actual.Dices, "역직렬화 이후 턴 순서 정보 중 주사위 상태가 바뀌면 안됩니다.");
        }, "역직렬화 이후 턴 순서 정보가 바뀌면 안됩니다.");
    }

    [TestMethod]
    [DataRow(4, DisplayName = "PerudoSessionState__같은_데이터를_두_번_직렬화하여_비교 - Sample 1")]
    [DataRow(5, DisplayName = "PerudoSessionState__같은_데이터를_두_번_직렬화하여_비교 - Sample 2")]
    [DataRow(6, DisplayName = "PerudoSessionState__같은_데이터를_두_번_직렬화하여_비교 - Sample 3")]
    [DataRow(7, DisplayName = "PerudoSessionState__같은_데이터를_두_번_직렬화하여_비교 - Sample 4")]
    [DataRow(8, DisplayName = "PerudoSessionState__같은_데이터를_두_번_직렬화하여_비교 - Sample 5")]
    public void PerudoSessionState__같은_데이터를_두_번_직렬화하여_비교__일치(int quantity)
    {
        // Arrange
        var state = GenState(4);
        
        // Act
        var binA = this.serializer.Serialize(state);
        var binB = this.serializer.Serialize(state);
        
        // Assert
        MyAssert.AreSequenceEquals(binA.ToArray(), binB.ToArray(), "같은 State를 직렬화하면 같은 바이너리가 나와야 합니다.");
    }
    
    [TestMethod]
    [DataRow(4, 5, DisplayName = "PerudoSessionState__서로_다른_데이터를_직렬화하여_비교 - Sample 1")]
    [DataRow(4, 6, DisplayName = "PerudoSessionState__서로_다른_데이터를_직렬화하여_비교 - Sample 2")]
    [DataRow(4, 7, DisplayName = "PerudoSessionState__서로_다른_데이터를_직렬화하여_비교 - Sample 3")]
    [DataRow(4, 8, DisplayName = "PerudoSessionState__서로_다른_데이터를_직렬화하여_비교 - Sample 4")]
    [DataRow(4, 9, DisplayName = "PerudoSessionState__서로_다른_데이터를_직렬화하여_비교 - Sample 5")]
    public void PerudoSessionState__서로_다른_데이터를_직렬화하여_비교__불일치(int quantityA, int quantityB)
    {
        // Arrange
        var a = GenState(quantityA);
        var b = GenState(quantityB);
        
        // Act
        var binA = this.serializer.Serialize(a);
        var binB = this.serializer.Serialize(b);
        
        // Assert
        MyAssert.AreSequenceNotEquals(binA?.ToArray(), binB?.ToArray(), "다른 값을 가진 State를 직렬화하면 다른 바이너리가 나와야 합니다.");
    }
}