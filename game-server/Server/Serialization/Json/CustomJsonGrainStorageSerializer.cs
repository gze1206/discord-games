using System.Text.Json;
using Orleans.Storage;

namespace DiscordGames.Server.Serialization.Json;

public class CustomJsonGrainStorageSerializer : IGrainStorageSerializer
{
    private readonly JsonSerializerOptions jsonSerializerOptions;

    public CustomJsonGrainStorageSerializer(JsonSerializerOptions jsonSerializerOptions)
    {
        this.jsonSerializerOptions = jsonSerializerOptions;
    }
    
    public BinaryData Serialize<T>(T input)
    {
        return new BinaryData(JsonSerializer.SerializeToUtf8Bytes(input, this.jsonSerializerOptions));
    }

    public T Deserialize<T>(BinaryData input)
    {
        return input.ToObjectFromJson<T>(this.jsonSerializerOptions)!;
    }
}