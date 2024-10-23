using System.Text;
using DiscordGames.Core.Memory;
using DiscordGames.Core.Net.Serialize;
using UnitTests.Utils;

namespace UnitTests.Core.Memory;

[TestClass, TestCategory("Serialization")]
public class BufferReadWriteTest
{
    [TestInitialize]
    public void Init()
    {
        Globals.Init();
    }
    
    [TestMethod]
    [DataRow((byte)1, DisplayName = "Sample 1")]
    [DataRow((byte)10, DisplayName = "Sample 2")]
    [DataRow((byte)100, DisplayName = "Sample 3")]
    [DataRow(byte.MinValue, DisplayName = "Sample 4")]
    [DataRow(byte.MaxValue, DisplayName = "Sample 5")]
    public void BufferReadWrite__Byte를_직렬화_후_다시_역직렬화하여_원본과_비교__일치(byte value)
    {
        // Arrange
        
        // Act
        var writer = new BufferWriter();
        var succeed = writer.Write(value);
        
        var written = writer.UsedTotal;
        var arr = new byte[written];
        writer.CopyTo(arr);
        writer.Dispose();

        var reader = new BufferReader(arr);
        var actual = reader.ReadByte();
        
        // Assert
        Assert.IsTrue(succeed);
        Assert.AreEqual(sizeof(byte), written);
        Assert.AreEqual(value, actual);
    }
    
    [TestMethod]
    [DataRow(10, DisplayName = "Sample 1")]
    [DataRow(-10, DisplayName = "Sample 2")]
    [DataRow(0, DisplayName = "Sample 3")]
    [DataRow(int.MinValue, DisplayName = "Sample 4")]
    [DataRow(int.MaxValue, DisplayName = "Sample 5")]
    public void BufferReadWrite__Int32를_직렬화_후_다시_역직렬화하여_원본과_비교__일치(int value)
    {
        // Arrange
        
        // Act
        var writer = new BufferWriter();
        var succeed = writer.Write(value);
        
        var written = writer.UsedTotal;
        var arr = new byte[written];
        writer.CopyTo(arr);
        writer.Dispose();

        var reader = new BufferReader(arr);
        var actual = reader.ReadInt32();
        
        // Assert
        Assert.IsTrue(succeed);
        Assert.AreEqual(sizeof(int), written);
        Assert.AreEqual(value, actual);
    }
    
    [TestMethod]
    [DataRow(10L, DisplayName = "Sample 1")]
    [DataRow(-10L, DisplayName = "Sample 2")]
    [DataRow(0L, DisplayName = "Sample 3")]
    [DataRow(long.MinValue, DisplayName = "Sample 4")]
    [DataRow(long.MaxValue, DisplayName = "Sample 5")]
    public void BufferReadWrite__Int64를_직렬화_후_다시_역직렬화하여_원본과_비교__일치(long value)
    {
        // Arrange
        
        // Act
        var writer = new BufferWriter();
        var succeed = writer.Write(value);
        
        var written = writer.UsedTotal;
        var arr = new byte[written];
        writer.CopyTo(arr);
        writer.Dispose();

        var reader = new BufferReader(arr);
        var actual = reader.ReadInt64();
        
        // Assert
        Assert.IsTrue(succeed);
        Assert.AreEqual(sizeof(long), written);
        Assert.AreEqual(value, actual);
    }
    
    [TestMethod]
    [DataRow("", DisplayName = "Sample 1")]
    [DataRow("a", DisplayName = "Sample 2")]
    [DataRow(null, DisplayName = "Sample 3")]
    [DataRow("abcdefghijklmnopqrstuvwxyz", DisplayName = "Sample 4")]
    [DataRow("UTB-8 인코딩 文字列", DisplayName = "Sample 5")]
    public void BufferReadWrite__String을_직렬화_후_다시_역직렬화하여_원본과_비교__일치(string? value)
    {
        // Arrange
        var expectedSize = (string.IsNullOrEmpty(value) ? 0 : Encoding.UTF8.GetByteCount(value)) + sizeof(int);
        
        // Act
        var writer = new BufferWriter();
        var succeed = writer.Write(value);
        
        var written = writer.UsedTotal;
        var arr = new byte[written];
        writer.CopyTo(arr);
        writer.Dispose();

        var reader = new BufferReader(arr);
        var actual = reader.ReadString();
        
        // Assert
        Assert.IsTrue(succeed);
        Assert.AreEqual(value == null, actual == null);
        Assert.AreEqual(expectedSize, written);
        Assert.AreEqual(value, actual);
    }
}