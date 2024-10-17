using System.Diagnostics.CodeAnalysis;

namespace UnitTests.TestClasses;

public class TestState
{
    public int ValueType { get; set; }
    public string ReferenceType { get; set; } = default!;
    public LinkedList<TestData> TestDataList { get; set; } = new();
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