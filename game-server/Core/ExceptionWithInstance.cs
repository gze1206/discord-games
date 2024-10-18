namespace DiscordGames.Core;

public abstract class ExceptionWithInstance<T> : Exception
    where T : ExceptionWithInstance<T>, new()
{
    protected ExceptionWithInstance() { }
    protected ExceptionWithInstance(string message) : base(message) { }

    private static Lazy<T> Lazy { get; } = new();
    public static T I => Lazy.Value;
}