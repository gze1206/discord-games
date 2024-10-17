using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Orleans.Serialization;
using Orleans.Storage;

namespace DiscordGames.Server.Serialization.Json;

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

public static class CustomJsonConverterExtension
{
    public static ISiloBuilder AddCustomJsonSerializer(this ISiloBuilder siloBuilder, CustomJsonConvertBuilder builder)
    {
        var jsonSerializerOptions = builder.BakeOptions();
        
        siloBuilder.Services
            .AddSerializer(serializerBuilder => serializerBuilder
                .AddJsonSerializer(
                    isSupported: builder.IsSupport,
                    jsonSerializerOptions: jsonSerializerOptions));

        siloBuilder.AddMemoryGrainStorageAsDefault(options =>
            options.GrainStorageSerializer = new CustomJsonGrainStorageSerializer(jsonSerializerOptions));

        return siloBuilder;
    }
}
