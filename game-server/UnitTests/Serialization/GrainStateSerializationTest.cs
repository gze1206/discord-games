using DiscordGames.Grain.Implements.GameSessions;
using DiscordGames.Grain.States;
using DiscordGames.Server.Serialization.Json;
using Orleans.Storage;
using UnitTests.Utils;

namespace UnitTests.Serialization;

[TestClass, TestCategory("Serialization")]
public class GrainStateSerializationTest
{
    private IGrainStorageSerializer serializer = default!;

    [TestInitialize]
    public void Init()
    {
        this.serializer = new CustomJsonGrainStorageSerializer(new CustomJsonConvertBuilder()
            .Add<LinkedListJsonConverter<PerudoSessionGrain.PlayerInfo>>()
            .BakeOptions());
    }

    [TestMethod]
    public void PerudoSessionStateTest()
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
            CurrentTurn = players.First(),
        };

        foreach (var playerId in players)
        {
            state.Players.Add(playerId);

            var info = new PerudoSessionGrain.PlayerInfo(playerId, 3);
            info.Roll();
            state.TurnOrder.AddLast(info);
        }

        var serialized = this.serializer.Serialize(state);
        var deserialized = this.serializer.Deserialize<PerudoSessionState>(serialized);
        
        Assert.IsNotNull(deserialized, "역직렬화 결과가 NULL이면 안됩니다.");
        MyAssert.AreSequenceEquals(state.Players, deserialized.Players, "역직렬화 이후 플레이어 목록이 바뀌면 안됩니다.");
        MyAssert.AreSequenceEquals(state.Spectators, deserialized.Spectators, "역직렬화 이후 관전자 목록이 바뀌면 안됩니다.");
        MyAssert.AreSequenceEquals(state.Players, deserialized.Players, "역직렬화 이후 플레이어 목록이 바뀌면 안됩니다.");
        Assert.AreEqual(state.HostUserId, deserialized.HostUserId, "역직렬화 이후 호스트 유저가 바뀌면 안됩니다.");
        Assert.AreEqual(state.IsInitialized, deserialized.IsInitialized, "역직렬화 이후 초기화 여부가 바뀌면 안됩니다.");
        Assert.AreEqual(state.IsPlaying, deserialized.IsPlaying, "역직렬화 이후 플레이 상태가 바뀌면 안됩니다.");
        Assert.AreEqual(state.IsClassicRule, deserialized.IsClassicRule, "역직렬화 이후 규칙 설정이 바뀌면 안됩니다.");
        Assert.AreEqual(state.CurrentTurn, deserialized.CurrentTurn, "역직렬화 이후 현재 턴 정보가 바뀌면 안됩니다.");
        
        MyAssert.AreSequenceEquals(state.TurnOrder, deserialized.TurnOrder, (expected, actual) =>
        {
            Assert.AreEqual(expected.UserId, actual.UserId, "역직렬화 이후 턴 순서 정보 중 UserId가 바뀌면 안됩니다.");
            Assert.AreEqual(expected.Life, actual.Life, "역직렬화 이후 턴 순서 정보 중 라이프가 바뀌면 안됩니다.");
            MyAssert.AreSequenceEquals(expected.Dices, actual.Dices, "역직렬화 이후 턴 순서 정보 중 주사위 상태가 바뀌면 안됩니다.");
        }, "역직렬화 이후 턴 순서 정보가 바뀌면 안됩니다.");
    }
}