using System.Text.Json;
using System.Text.Json.Serialization;

namespace DiscordGames.Grain.Serialization.Json;

public class CustomJsonConvertBuilder
{
    private readonly List<ICustomJsonConverter> converters = new();
    
    public CustomJsonConvertBuilder Add<T>() where T : JsonConverter, ICustomJsonConverter, new()
    {
        this.converters.Add(new T());
        return this;
    }
    
    public bool IsSupport(Type type)
    {
        foreach (var converter in this.converters)
        {
            if (converter.IsSupport(type)) return true;
        }

        return false;
    }

    // ReSharper disable once MemberCanBePrivate.Global
    public JsonSerializerOptions BakeOptions()
    {
        var options = new JsonSerializerOptions { WriteIndented = true };

        foreach (var converter in this.converters)
        {
            options.Converters.Add((JsonConverter)converter);
        }

        return options;
    }
}
