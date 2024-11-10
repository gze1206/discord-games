using System.Text.Json;
using DiscordGames.Grains.Serialization.Json;
using UnitTests.TestClasses;
using UnitTests.Utils;

namespace UnitTests.Server.Serialization.Json;

[TestClass, TestCategory("Serialization"), TestCategory("Grain")]
public class JsonSerializationTest
{
    private static readonly TestData[] TestDataArray =
    {
        new(1, "A"),
        new(2, "B"),
        new(3, "C"),
    };
    
    [TestMethod]
    public void LinkedListConverter__정수형_링크드리스트_직렬화_후_다시_역직렬화하여_원본과_비교__일치()
    {
        LinkedListConverterEqualTest(1, 2, 3);
    }
    
    [TestMethod]
    public void LinkedListConverter__문자열_링크드리스트_직렬화_후_다시_역직렬화하여_원본과_비교__일치()
    {
        LinkedListConverterEqualTest("A", "B", "C");
    }
    
    [TestMethod]
    public void LinkedListConverter__레코드_링크드리스트_직렬화_후_다시_역직렬화하여_원본과_비교__일치()
    {
        LinkedListConverterEqualTest(TestDataArray);
    }

    private static void LinkedListConverterEqualTest<T>(params T[] data)
    {
        // Arrange
        var options = new CustomJsonConvertBuilder()
            .Add<LinkedListJsonConverter<T>>()
            .BakeOptions();

        var list = new LinkedList<T>(data);

        // Act
        var json = JsonSerializer.Serialize(list, options);
        var converted = JsonSerializer.Deserialize<LinkedList<T>>(json, options);
        
        // Assert
        Assert.IsNotNull(converted, "JSON 변환 이후 NULL이 되면 안됩니다.");
        MyAssert.AreSequenceEquals(list, converted, "JSON 변환 이후 크기나 내용이 달라지면 안됩니다.");
    }

    [TestMethod]
    [DataRow(new[] { 1 }, DisplayName = "LinkedListConverter__같은_데이터를_두_번_직렬화하여_비교 - Sample 1")]
    [DataRow(new[] { 1, 2, 3 }, DisplayName = "LinkedListConverter__같은_데이터를_두_번_직렬화하여_비교 - Sample 2")]
    [DataRow(new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 }, DisplayName = "LinkedListConverter__같은_데이터를_두_번_직렬화하여_비교 - Sample 3")]
    public void LinkedListConverter__같은_데이터를_두_번_직렬화하여_비교__일치(int[] a)
    {
        // Arrange
        var options = new CustomJsonConvertBuilder()
            .Add<LinkedListJsonConverter<int>>()
            .BakeOptions();

        var list = new LinkedList<int>(a);
        
        // Act
        var jsonA = JsonSerializer.Serialize(list, options);
        var jsonB = JsonSerializer.Serialize(list, options);

        // Assert
        Assert.AreEqual(jsonA, jsonB);
    }
    
    [TestMethod]
    [DataRow(
        new[] { 1 },
        new[] { 2 },
        DisplayName = "LinkedListConverter__서로_다른_데이터를_직렬화하여_비교 - Sample 1"
    )]
    [DataRow(
        new[] { 1, 2, 3 },
        new[] { 4, 5, 6 },
        DisplayName = "LinkedListConverter__서로_다른_데이터를_직렬화하여_비교 - Sample 2"
    )]
    [DataRow(
        new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 },
        new[] { 9, 8, 7, 6, 5, 4, 3, 2, 1 },
        DisplayName = "LinkedListConverter__서로_다른_데이터를_직렬화하여_비교 - Sample 3"
    )]
    public void LinkedListConverter__서로_다른_데이터를_직렬화하여_비교__불일치(int[] a, int[] b)
    {
        // Arrange
        var options = new CustomJsonConvertBuilder()
            .Add<LinkedListJsonConverter<int>>()
            .BakeOptions();

        var listA = new LinkedList<int>(a);
        var listB = new LinkedList<int>(b);
        
        // Act
        var jsonA = JsonSerializer.Serialize(listA, options);
        var jsonB = JsonSerializer.Serialize(listB, options);

        // Assert
        Assert.AreNotEqual(jsonA, jsonB);
    }

    [TestMethod]
    public void CustomJsonGrainStorageSerializer__테스트_데이터_직렬화_후_다시_역직렬화하여_비교__일치()
    {
        // Arrange
        var state = new TestState
        {
            ValueType = 123,
            ReferenceType = "ABC",
            TestDataList = new LinkedList<TestData>(TestDataArray),
        };
        
        var serializer = new CustomJsonGrainStorageSerializer(new CustomJsonConvertBuilder()
            .Add<LinkedListJsonConverter<TestState>>()
            .BakeOptions());

        // Act
        var serialized = serializer.Serialize(state);
        var deserialized = serializer.Deserialize<TestState>(serialized);
        
        // Assert
        Assert.IsNotNull(deserialized, "역직렬화 이후에 NULL이 되면 안됩니다.");
        Assert.AreEqual(state.ValueType, deserialized.ValueType, "역직렬화 이후에 ValueType 값이 달라지면 안됩니다.");
        Assert.AreEqual(state.ReferenceType, deserialized.ReferenceType, "역직렬화 이후에 ReferenceType 값이 달라지면 안됩니다.");
        MyAssert.AreSequenceEquals(state.TestDataList, deserialized.TestDataList, "역직렬화 이후에 LinkedList<T> 값이 달라지면 안됩니다.");
    }
}