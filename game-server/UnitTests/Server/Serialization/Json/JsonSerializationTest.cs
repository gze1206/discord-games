using System.Text.Json;
using DiscordGames.Server.Serialization.Json;
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
    public void LinkedListConverter_IntSerializeAndDeserialize_AreEqual()
    {
        LinkedListConverterEqualTest(1, 2, 3);
    }
    
    [TestMethod]
    public void LinkedListConverter_StringSerializeAndDeserialize_AreEqual()
    {
        LinkedListConverterEqualTest("A", "B", "C");
    }
    
    [TestMethod]
    public void LinkedListConverter_RecordSerializeAndDeserialize_AreEqual()
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
    [DataRow(new[] { 1 }, DisplayName = "Sample 1")]
    [DataRow(new[] { 1, 2, 3 }, DisplayName = "Sample 2")]
    [DataRow(new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 }, DisplayName = "Sample 3")]
    public void LinkedListConverter_CompareJson_AreEqual(int[] a)
    {
        // Arrange
        var options = new CustomJsonConvertBuilder()
            .Add<LinkedListJsonConverter<int>>()
            .BakeOptions();

        var listA = new LinkedList<int>(a);
        var listB = new LinkedList<int>(a);
        
        // Act
        var jsonA = JsonSerializer.Serialize(listA, options);
        var jsonB = JsonSerializer.Serialize(listB, options);

        // Assert
        Assert.AreEqual(jsonA, jsonB);
    }
    
    [TestMethod]
    [DataRow(
        new[] { 1 },
        new[] { 2 },
        DisplayName = "Sample 1"
    )]
    [DataRow(
        new[] { 1, 2, 3 },
        new[] { 4, 5, 6 },
        DisplayName = "Sample 2"
    )]
    [DataRow(
        new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 },
        new[] { 9, 8, 7, 6, 5, 4, 3, 2, 1 },
        DisplayName = "Sample 3"
    )]
    public void LinkedListConverter_CompareJson_AreNotEqual(int[] a, int[] b)
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
    public void CustomJsonGrainStorageSerializer_SerializeAndDeserialize_AreEqual()
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