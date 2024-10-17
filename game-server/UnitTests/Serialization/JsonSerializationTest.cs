using System.Text.Json;
using DiscordGames.Server.Serialization.Json;
using UnitTests.TestClasses;
using UnitTests.Utils;

namespace UnitTests.Serialization;

[TestClass, TestCategory("Serialization")]
public class JsonSerializationTest
{
    private static readonly TestData[] TestDataArray =
    {
        new(1, "A"),
        new(2, "B"),
        new(3, "C"),
    };
    
    [TestMethod]
    public void LinkedListConverterTest()
    {
        LinkedListConverterTestBase(1, 2, 3);
        LinkedListConverterTestBase("A", "B", "C");
        LinkedListConverterTestBase(TestDataArray);
    }

    private static void LinkedListConverterTestBase<T>(params T[] data)
    {
        var options = new CustomJsonConvertBuilder()
            .Add<LinkedListJsonConverter<T>>()
            .BakeOptions();

        var list = new LinkedList<T>(data);

        var json = JsonSerializer.Serialize(list, options);
        var converted = JsonSerializer.Deserialize<LinkedList<T>>(json, options);
        
        Assert.IsNotNull(converted, "JSON 변환 이후 NULL이 되면 안 됩니다.");
        MyAssert.AreSequenceEquals(list, converted, "JSON 변환 이후 크기나 내용이 달라지면 안 됩니다.");
    }

    [TestMethod]
    public void StateSerializationTest()
    {
        var state = new TestState
        {
            ValueType = 123,
            ReferenceType = "ABC",
            TestDataList = new LinkedList<TestData>(TestDataArray),
        };
        
        var serializer = new CustomJsonGrainStorageSerializer(new CustomJsonConvertBuilder()
            .Add<LinkedListJsonConverter<TestState>>()
            .BakeOptions());

        var serialized = serializer.Serialize(state);
        var deserialized = serializer.Deserialize<TestState>(serialized);
        
        Assert.IsNotNull(deserialized, "역직렬화 이후에 NULL이 되면 안 됩니다.");
        Assert.AreEqual(state.ValueType, deserialized.ValueType, "역직렬화 이후에 ValueType 값이 달라지면 안 됩니다.");
        Assert.AreEqual(state.ReferenceType, deserialized.ReferenceType, "역직렬화 이후에 ReferenceType 값이 달라지면 안 됩니다.");
        MyAssert.AreSequenceEquals(state.TestDataList, deserialized.TestDataList, "역직렬화 이후에 LinkedList<T> 값이 달라지면 안 됩니다.");
    }
}