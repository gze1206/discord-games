using DiscordGames.Grains.Serialization.Json;
using Orleans.Serialization;

namespace DiscordGames.Silo.Serialization.Json;

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