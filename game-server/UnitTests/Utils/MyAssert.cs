namespace UnitTests.Utils;

public static class MyAssert
{
    public static void AreSequenceEquals<T>(IEnumerable<T>? expected, IEnumerable<T>? actual)
        => AreSequenceEquals(expected?.ToArray(), actual?.ToArray(), string.Empty, null);
    
    public static void AreSequenceEquals<T>(IEnumerable<T>? expected, IEnumerable<T>? actual, Action<T, T> action)
        => AreSequenceEquals(expected?.ToArray(), actual?.ToArray(), action, string.Empty, null);
    
    public static void AreSequenceEquals<T>(T[]? expected, T[]? actual)
        => AreSequenceEquals(expected, actual, string.Empty, null);
    
    public static void AreSequenceEquals<T>(T[]? expected, T[]? actual, Action<T, T> action)
        => AreSequenceEquals(expected, actual, action, string.Empty, null);
    
    public static void AreSequenceEquals<T>(IEnumerable<T>? expected, IEnumerable<T>? actual,
        string message)
        => AreSequenceEquals(expected?.ToArray(), actual?.ToArray(), message, null);
    
    public static void AreSequenceEquals<T>(IEnumerable<T>? expected, IEnumerable<T>? actual, Action<T, T> action,
        string message)
        => AreSequenceEquals(expected?.ToArray(), actual?.ToArray(), action, message, null);
    
    public static void AreSequenceEquals<T>(T[]? expected, T[]? actual, string message)
        => AreSequenceEquals(expected, actual, message, null);
    
    public static void AreSequenceEquals<T>(T[]? expected, T[]? actual, Action<T, T> action, string message)
        => AreSequenceEquals(expected, actual, action, message, null);

    public static void AreSequenceEquals<T>(IEnumerable<T>? expected, IEnumerable<T>? actual,
        string message, params object?[]? parameters)
        => AreSequenceEquals(expected?.ToArray(), actual?.ToArray(), message, parameters);
    
    public static void AreSequenceEquals<T>(IEnumerable<T>? expected, IEnumerable<T>? actual, Action<T, T> action,
        string message, params object?[]? parameters)
        => AreSequenceEquals(expected?.ToArray(), actual?.ToArray(), action, message, parameters);
    
    public static void AreSequenceEquals<T>(T[]? expected, T[]? actual,
        string message, params object?[]? parameters)
    {
        Assert.AreEqual(expected == null, actual == null, message, parameters);

        if (expected == null) return;
        
        Assert.AreEqual(expected.Length, actual!.Length, message, parameters);

        for (int i = 0, len = expected.Length; i < len; i++)
        {
            Assert.AreEqual(expected[i], actual[i], message, parameters);
        }
    }
    
    public static void AreSequenceEquals<T>(T[]? expected, T[]? actual, Action<T, T> action,
        string message, params object?[]? parameters)
    {
        Assert.AreEqual(expected == null, actual == null, message, parameters);

        if (expected == null) return;
        
        Assert.AreEqual(expected.Length, actual!.Length, message, parameters);

        for (int i = 0, len = expected.Length; i < len; i++)
        {
            action(expected[i], actual[i]);
        }
    }
}