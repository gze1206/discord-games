using System.Diagnostics.CodeAnalysis;

// ReSharper disable once CheckNamespace
namespace UnitTests.TestClasses;

public class TestState
{
    public int ValueType { get; init; }
    public string ReferenceType { get; init; } = default!;
    public LinkedList<TestData> TestDataList { get; init; } = new();
}

[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public record TestData
{
    public int ValueType { get; set; }
    public string ReferenceType { get; set; } = default!;

    public TestData(int valueType, string referenceType)
    {
        this.ValueType = valueType;
        this.ReferenceType = referenceType;
    }
}