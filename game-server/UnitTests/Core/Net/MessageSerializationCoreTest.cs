using System.Text;
using DiscordGames.Core.Memory;
using DiscordGames.Core.Net;
using DiscordGames.Core.Net.Message;
using DiscordGames.Core.Net.Serialize;
using UnitTests.TestClasses;
using UnitTests.Utils;

// ReSharper disable StringLiteralTypo

namespace UnitTests.Core.Net;

[TestClass, TestCategory("Serialization"), TestCategory("Message")]
public class MessageSerializationCoreTest
{
    [TestMethod]
    [DataRow("123456789", 0xCBF43926U, DisplayName = "CalcChecksum__사전_계산된_Checksum과_비교 - Sample 1 - \"123456789\"")]
    [DataRow("TEST DATA", 0x560B9F59U, DisplayName = "CalcChecksum__사전_계산된_Checksum과_비교 - Sample 2 - \"TEST DATA\"")]
    [DataRow("Short", 0x4EE9BFA6U, DisplayName = "CalcChecksum__사전_계산된_Checksum과_비교 - Sample 3 - Short data")]
    [DataRow("A QUITE LONG DATA FOR CRC CHECKSUM", 0xE76225B4U, DisplayName = "CalcChecksum__사전_계산된_Checksum과_비교 - Sample 4 - Long data")]
    [DataRow(
        "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Quisque non imperdiet purus. Quisque maximus quis ligula ac feugiat. Ut imperdiet sit amet turpis in semper. Nam at consectetur orci. In ex enim, ultrices vitae scelerisque a, ornare at magna libero.",
        0x5C2EF6E0U,
        DisplayName = "CalcChecksum__사전_계산된_Checksum과_비교 - Sample 5 - Lorem ipsum (256 bytes)")]
    public void CalcChecksum__사전_계산된_Checksum과_비교__일치(string source, uint expected)
    {
        // Arrange
        
        // Act
        var actual = MessageSerializer.CalcChecksum(Encoding.ASCII.GetBytes(source));
        
        // Assert
        Assert.AreEqual(expected, actual);
    }

    [TestMethod]
    [DataRow("123456789", "234567890", DisplayName = "CalcChecksum__서로_다른_데이터로_Checksum_계산_후_비교 - Sample 1")]
    [DataRow("TEST DATA", "test data", DisplayName = "CalcChecksum__서로_다른_데이터로_Checksum_계산_후_비교 - Sample 2")]
    [DataRow("checksums should not equals", "with diffrent payload", DisplayName = "CalcChecksum__서로_다른_데이터로_Checksum_계산_후_비교 - Sample 3")]
    [DataRow("CRC", "CHECKSUM", DisplayName = "CalcChecksum__서로_다른_데이터로_Checksum_계산_후_비교 - Sample 4")]
    [DataRow("439583490573409578", "sahfskldjhvjksldbvlsjkdbv", DisplayName = "CalcChecksum__서로_다른_데이터로_Checksum_계산_후_비교 - Sample 5")]
    public void CalcChecksum__서로_다른_데이터로_Checksum_계산_후_비교__불일치(string a, string b)
    {
        // Arrange
        
        // Act
        var aChecksum = MessageSerializer.CalcChecksum(Encoding.ASCII.GetBytes(a));
        var bChecksum = MessageSerializer.CalcChecksum(Encoding.ASCII.GetBytes(b));
        
        // Assert
        Assert.AreNotEqual(aChecksum, bChecksum);
    }

    [TestMethod]
    public async Task MessageSerializer__직렬화_후_다시_역직렬화하여_원본과_비교__일치()
    {
        // Arrange
        var handler = new PingTestHandler();

        var header = new MessageHeader(MessageSerializer.SchemeVersion, MessageChannel.Global, MessageType.Ping);
        var expected = new PingMessage(
            ref header,
            DateTime.UtcNow.Ticks);

        // Act
        var binary = MessageSerializer.Write(ref expected);
        
        var reader = new BufferReader(binary);
        reader.AdvanceWrite(binary.Length);

        await reader.ReadAndHandleMessage(handler);

        var isSucceed = await handler.Wait();
        
        // Assert
        Assert.IsTrue(isSucceed, "메시지 역직렬화에 성공해야 합니다");
        Assert.AreEqual(expected, handler.Actual);
    }
    
    [TestMethod]
    public async Task MessageSerializer__직렬화_후_Checksum_변조하고_다시_역직렬화하여_원본과_비교__예외_MessageChecksumException()
    {
        // Arrange
        var handler = new PingTestHandler(100);

        var header = new MessageHeader(MessageSerializer.SchemeVersion, MessageChannel.Global, MessageType.Ping);
        var expected = new PingMessage(
            ref header,
            DateTime.UtcNow.Ticks);

        // Act
        try
        {
            var binary = MessageSerializer.Write(ref expected);
            binary[^1] = (byte)((binary[^1] + 1) % byte.MaxValue);

            var reader = new BufferReader(binary);
            reader.AdvanceWrite(binary.Length);

            await reader.ReadAndHandleMessage(handler);
        }
        catch (MessageChecksumException)
        {
            // Assert
            Assert.IsFalse(await handler.Wait());
            Assert.AreNotEqual(expected, handler.Actual);
        }

    }
    
    [TestMethod]
    public async Task MessageSerializer__직렬화_후_Checksum_변조하고_다시_역직렬화하여_원본과_비교__예외_MessageSchemeVersionException()
    {
        // Arrange
        var handler = new PingTestHandler(100);

        var header = new MessageHeader(MessageSerializer.SchemeVersion - 1, MessageChannel.Global, MessageType.Ping);
        var expected = new PingMessage(
            ref header,
            DateTime.UtcNow.Ticks);

        // Act
        try
        {
            var binary = MessageSerializer.Write(ref expected);
            
            var reader = new BufferReader(binary);
            reader.AdvanceWrite(binary.Length);

            await reader.ReadAndHandleMessage(handler);
        }
        catch (MessageSchemeVersionException)
        {
            // Assert
            Assert.IsFalse(await handler.Wait());
            Assert.AreNotEqual(expected, handler.Actual);
        }
    }

    [TestMethod]
    public void MessageSerializer__서로_다른_메시지를_직렬화하여_비교__불일치()
    {
        // Arrange
        var header = new MessageHeader(MessageSerializer.SchemeVersion, MessageChannel.Global, MessageType.Ping);
        var a = new PingMessage(
            ref header,
            DateTime.UtcNow.Ticks - 5);
        var b = new PingMessage(
            ref header,
            DateTime.UtcNow.Ticks + 5);
        
        // Act
        var binA = MessageSerializer.Write(ref a);
        var binB = MessageSerializer.Write(ref b);
        
        // Assert
        MyAssert.AreSequenceNotEquals(binA, binB);
    }

    [TestMethod]
    public void MessageSerializer__같은_메시지를_두_번_직렬화하여_비교__일치()
    {
        // Arrange
        var header = new MessageHeader(MessageSerializer.SchemeVersion, MessageChannel.Global, MessageType.Ping);
        var message = new PingMessage(
            ref header,
            DateTime.UtcNow.Ticks);
        
        // Act
        var binA = MessageSerializer.Write(ref message);
        var binB = MessageSerializer.Write(ref message);
        
        // Assert
        MyAssert.AreSequenceEquals(binA, binB);
    }

    private class PingTestHandler : TestVirtualMessageHandler
    {
        private readonly TaskCompletionSource<bool> pingReceived = new();
        private readonly int timeout;
        
        public PingMessage Actual { get; private set; }

        public PingTestHandler(int timeout = 1000)
        {
            this.timeout = timeout;
        }
        
        public override ValueTask OnPing(PingMessage message)
        {
            this.pingReceived.SetResult(true);
            this.Actual = message;
            return base.OnPing(message);
        }

        public async Task<bool> Wait()
            => await Task.WhenAny(this.pingReceived.Task, Task.Delay(this.timeout)) == this.pingReceived.Task;
    }
}