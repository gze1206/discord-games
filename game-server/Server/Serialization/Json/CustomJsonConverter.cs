using System.Text.Json.Serialization;

namespace DiscordGames.Server.Serialization.Json;

public interface ICustomJsonConverter
{
    bool IsSupport(Type type);
}

public abstract class CustomJsonConverter<T> : JsonConverter<T>, ICustomJsonConverter
{
    public bool IsSupport(Type type) => type == typeof(T);
}