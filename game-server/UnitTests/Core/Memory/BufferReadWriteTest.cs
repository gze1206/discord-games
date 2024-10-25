using System.Text;
using DiscordGames.Core.Memory;
using DiscordGames.Core.Net.Serialize;

namespace UnitTests.Core.Memory;

[TestClass, TestCategory("Serialization")]
public class BufferReadWriteTest
{
    [TestMethod]
    [DataRow((byte)1, DisplayName = "BufferReadWrite__Byte를_직렬화_후_다시_역직렬화하여_원본과_비교 - Sample 1")]
    [DataRow((byte)10, DisplayName = "BufferReadWrite__Byte를_직렬화_후_다시_역직렬화하여_원본과_비교 - Sample 2")]
    [DataRow((byte)100, DisplayName = "BufferReadWrite__Byte를_직렬화_후_다시_역직렬화하여_원본과_비교 - Sample 3")]
    [DataRow(byte.MinValue, DisplayName = "BufferReadWrite__Byte를_직렬화_후_다시_역직렬화하여_원본과_비교 - Sample 4")]
    [DataRow(byte.MaxValue, DisplayName = "BufferReadWrite__Byte를_직렬화_후_다시_역직렬화하여_원본과_비교 - Sample 5")]
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
    [DataRow(10, DisplayName = "BufferReadWrite__Int32를_직렬화_후_다시_역직렬화하여_원본과_비교 - Sample 1")]
    [DataRow(-10, DisplayName = "BufferReadWrite__Int32를_직렬화_후_다시_역직렬화하여_원본과_비교 - Sample 2")]
    [DataRow(0, DisplayName = "BufferReadWrite__Int32를_직렬화_후_다시_역직렬화하여_원본과_비교 - Sample 3")]
    [DataRow(int.MinValue, DisplayName = "BufferReadWrite__Int32를_직렬화_후_다시_역직렬화하여_원본과_비교 - Sample 4")]
    [DataRow(int.MaxValue, DisplayName = "BufferReadWrite__Int32를_직렬화_후_다시_역직렬화하여_원본과_비교 - Sample 5")]
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
    [DataRow(10L, DisplayName = "BufferReadWrite__Int64를_직렬화_후_다시_역직렬화하여_원본과_비교 - Sample 1")]
    [DataRow(-10L, DisplayName = "BufferReadWrite__Int64를_직렬화_후_다시_역직렬화하여_원본과_비교 - Sample 2")]
    [DataRow(0L, DisplayName = "BufferReadWrite__Int64를_직렬화_후_다시_역직렬화하여_원본과_비교 - Sample 3")]
    [DataRow(long.MinValue, DisplayName = "BufferReadWrite__Int64를_직렬화_후_다시_역직렬화하여_원본과_비교 - Sample 4")]
    [DataRow(long.MaxValue, DisplayName = "BufferReadWrite__Int64를_직렬화_후_다시_역직렬화하여_원본과_비교 - Sample 5")]
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
    [DataRow("", DisplayName = "BufferReadWrite__String을_직렬화_후_다시_역직렬화하여_원본과_비교 - Sample 1 (Empty)")]
    [DataRow("a", DisplayName = "BufferReadWrite__String을_직렬화_후_다시_역직렬화하여_원본과_비교 - Sample 2 (Single character)")]
    [DataRow(null, DisplayName = "BufferReadWrite__String을_직렬화_후_다시_역직렬화하여_원본과_비교 - Sample 3 (NULL)")]
    [DataRow("abcdefghijklmnopqrstuvwxyz", DisplayName = "BufferReadWrite__String을_직렬화_후_다시_역직렬화하여_원본과_비교 - Sample 4")]
    [DataRow("UTB-8 인코딩 文字列", DisplayName = "BufferReadWrite__String을_직렬화_후_다시_역직렬화하여_원본과_비교 - Sample 5")]
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