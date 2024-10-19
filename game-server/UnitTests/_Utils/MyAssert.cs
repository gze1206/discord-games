// ReSharper disable once CheckNamespace
namespace UnitTests.Utils;

public static class MyAssert
{
    #region AreSequenceEquals
    
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
    
    #endregion // AreSequenceEquals
    
    #region AreSequenceNotEquals
    
    public static void AreSequenceNotEquals<T>(IEnumerable<T>? expected, IEnumerable<T>? actual) where T : IEquatable<T>
        => AreSequenceNotEquals(expected?.ToArray(), actual?.ToArray(), string.Empty, null);
    
    public static void AreSequenceNotEquals<T>(IEnumerable<T>? expected, IEnumerable<T>? actual, Action<T, T> action)
        => AreSequenceNotEquals(expected?.ToArray(), actual?.ToArray(), action, string.Empty, null);
    
    public static void AreSequenceNotEquals<T>(T[]? expected, T[]? actual) where T : IEquatable<T>
        => AreSequenceNotEquals(expected, actual, string.Empty, null);
    
    public static void AreSequenceNotEquals<T>(T[]? expected, T[]? actual, Action<T, T> action)
        => AreSequenceNotEquals(expected, actual, action, string.Empty, null);
    
    public static void AreSequenceNotEquals<T>(IEnumerable<T>? expected, IEnumerable<T>? actual,
        string message) where T : IEquatable<T>
        => AreSequenceNotEquals(expected?.ToArray(), actual?.ToArray(), message, null);
    
    public static void AreSequenceNotEquals<T>(IEnumerable<T>? expected, IEnumerable<T>? actual, Action<T, T> action,
        string message)
        => AreSequenceNotEquals(expected?.ToArray(), actual?.ToArray(), action, message, null);
    
    public static void AreSequenceNotEquals<T>(T[]? expected, T[]? actual, string message) where T : IEquatable<T>
        => AreSequenceNotEquals(expected, actual, message, null);
    
    public static void AreSequenceNotEquals<T>(T[]? expected, T[]? actual, Action<T, T> action, string message)
        => AreSequenceNotEquals(expected, actual, action, message, null);

    public static void AreSequenceNotEquals<T>(IEnumerable<T>? expected, IEnumerable<T>? actual,
        string message, params object?[]? parameters) where T : IEquatable<T>
        => AreSequenceNotEquals(expected?.ToArray(), actual?.ToArray(), message, parameters);
    
    public static void AreSequenceNotEquals<T>(IEnumerable<T>? expected, IEnumerable<T>? actual, Action<T, T> action,
        string message, params object?[]? parameters)
        => AreSequenceNotEquals(expected?.ToArray(), actual?.ToArray(), action, message, parameters);
    
    public static void AreSequenceNotEquals<T>(T[]? expected, T[]? actual,
        string message, params object?[]? parameters) where T : IEquatable<T>
    {
        if ((expected == null) != (actual == null)) return;

        if (expected == null && actual == null)
        {
            Assert.Fail(message, parameters);
            return;
        }

        if (expected?.Length != actual?.Length) return;

        for (int i = 0, len = expected!.Length; i < len; i++)
        {
            if (!expected[i].Equals(actual![i])) return;
        }
        
        Assert.Fail(message, parameters);
    }
    
    public static void AreSequenceNotEquals<T>(T[]? expected, T[]? actual, Action<T, T> action,
        string message, params object?[]? parameters)
    {
        if ((expected == null) != (actual == null)) return;

        if (expected == null && actual == null)
        {
            Assert.Fail(message, parameters);
            return;
        }

        if (expected?.Length != actual?.Length) return;
        
        for (int i = 0, len = expected!.Length; i < len; i++)
        {
            action(expected[i], actual![i]);
        }
    }
    
    #endregion // AreSequenceNotEquals
}