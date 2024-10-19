using System.Text;
using DiscordGames.Core.Net;
using DiscordGames.Core.Net.Message;
using UnitTests.TestClasses;
using UnitTests.Utils;

// ReSharper disable StringLiteralTypo

namespace UnitTests.Core.Net;

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
    public void CalcChecksum_WithPreCalculatedData_AreEqual(string source, uint expected)
    {
        // Arrange
        
        // Act
        var actual = MessageSerializer.CalcChecksum(Encoding.ASCII.GetBytes(source));
        
        // Assert
        Assert.AreEqual(expected, actual);
    }

    [TestMethod]
    [DataRow("123456789", "234567890", DisplayName = "Sample 1")]
    [DataRow("TEST DATA", "test data", DisplayName = "Sample 2")]
    [DataRow("checksums should not equals", "with diffrent payload", DisplayName = "Sample 3")]
    [DataRow("CRC", "CHECKSUM", DisplayName = "Sample 4")]
    [DataRow("439583490573409578", "sahfskldjhvjksldbvlsjkdbv", DisplayName = "Sample 5")]
    public void CalcChecksum_WithDiffPayload_AreNotEqual(string a, string b)
    {
        // Arrange
        
        // Act
        var aChecksum = MessageSerializer.CalcChecksum(Encoding.ASCII.GetBytes(a));
        var bChecksum = MessageSerializer.CalcChecksum(Encoding.ASCII.GetBytes(b));
        
        // Assert
        Assert.AreNotEqual(aChecksum, bChecksum);
    }

    [TestMethod]
    public async Task MessageSerializer_WriteAndRead_AreEqual()
    {
        // Arrange
        var handler = new PingTestHandler();

        var expected = new PingMessage
        {
            Header = new MessageHeader(1, MessageChannel.Global, MessageType.Ping),
            UtcTicks = DateTime.UtcNow.Ticks,
        };

        // Act
        var binary = expected.Write();
        MessageSerializer.Read(binary, handler);

        var isSucceed = await handler.Wait();
        
        // Assert
        Assert.IsTrue(isSucceed, "메시지 역직렬화에 성공해야 합니다");
        Assert.AreEqual(expected, handler.Actual);
    }

    [TestMethod]
    public void MessageSerializer_CompareBinaries_AreNotEqual()
    {
        // Arrange
        var a = new PingMessage
        {
            Header = new MessageHeader(1, MessageChannel.Global, MessageType.Ping),
            UtcTicks = DateTime.UtcNow.Ticks - 5,
        };
        var b = new PingMessage
        {
            Header = new MessageHeader(1, MessageChannel.Global, MessageType.Ping),
            UtcTicks = DateTime.UtcNow.Ticks + 5,
        };
        
        // Act
        var binA = a.Write();
        var binB = b.Write();
        
        // Assert
        MyAssert.AreSequenceNotEquals(binA, binB);
    }

    [TestMethod]
    public void MessageSerializer_CompareBinaries_AreEqual()
    {
        // Arrange
        var message = new PingMessage
        {
            Header = new MessageHeader(1, MessageChannel.Global, MessageType.Ping),
            UtcTicks = DateTime.UtcNow.Ticks,
        };
        
        // Act
        var binA = message.Write();
        var binB = message.Write();
        
        // Assert
        MyAssert.AreSequenceEquals(binA, binB);
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