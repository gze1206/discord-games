using System.Text;
using DiscordGames.Core.Net;
using DiscordGames.Core.Net.Message;
using UnitTests.TestClasses;

// ReSharper disable StringLiteralTypo

namespace UnitTests.Serialization;

[TestClass, TestCategory("Serialization"), TestCategory("Message")]
public class MessageSerializationCoreTest
{
    [TestMethod]
    [DataRow("123456789", 0xCBF43926U, DisplayName = "Sample 1 - \"123456789\"")]
    [DataRow("TEST DATA", 0x560B9F59U, DisplayName = "Sample 2 - \"TEST DATA\"")]
    [DataRow("Short", 0x4EE9BFA6U, DisplayName = "Sample 3 - Short data")]
    [DataRow("A QUITE LONG DATA FOR CRC CHECKSUM", 0xE76225B4U, DisplayName = "Sample 4 - Long data")]
    [DataRow(
        "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Quisque non imperdiet purus. Quisque maximus quis ligula ac feugiat. Ut imperdiet sit amet turpis in semper. Nam at consectetur orci. In ex enim, ultrices vitae scelerisque a, ornare at magna libero.",
        0x5C2EF6E0U,
        DisplayName = "Sample 5 - Lorem ipsum (256 bytes)")]
    public void ChecksumTest(string source, uint expected)
    {
        var actual = MessageSerializer.CalcChecksum(Encoding.ASCII.GetBytes(source));
        Assert.AreEqual(expected, actual);
    }

    [TestMethod]
    public async Task SerializeTest()
    {
        var handler = new PingTestHandler();

        var expected = new PingMessage
        {
            Header = new MessageHeader(1, MessageChannel.Global, MessageType.Ping),
            UtcTicks = DateTime.UtcNow.Ticks
        };

        var binary = expected.Write();
        MessageSerializer.Read(binary, handler);

        var isSucceed = await handler.Wait();
        Assert.IsTrue(isSucceed, "메시지 역직렬화에 성공해야 합니다");
        Assert.AreEqual(expected, handler.Actual);
    }

    private class PingTestHandler : TestVirtualMessageHandler
    {
        private readonly TaskCompletionSource<bool> pingReceived = new();
        
        public PingMessage Actual { get; private set; }
        
        public override Task OnPing(PingMessage message)
        {
            this.pingReceived.SetResult(true);
            this.Actual = message;
            return base.OnPing(message);
        }

        public async Task<bool> Wait()
            => await Task.WhenAny(this.pingReceived.Task, Task.Delay(1000)) == this.pingReceived.Task;
    }
}