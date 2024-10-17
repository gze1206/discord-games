using System.Text.Json;

namespace DiscordGames.Server.Serialization.Json;

public class LinkedListJsonConverter<T> : CustomJsonConverter<LinkedList<T>>
{
    public override LinkedList<T>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartArray)
            throw new JsonException($"Expected StartArray, but {reader.TokenType.ToString()} actually");
        reader.Read();
        
        var result = new LinkedList<T>();

        while (reader.TokenType != JsonTokenType.EndArray)
        {
            result.AddLast(JsonSerializer.Deserialize<T>(ref reader, options) ?? throw new JsonException($"Expected {typeof(T).Name}, but {reader.TokenType} actually"));
            reader.Read();
        }

        return result;
    }

    public override void Write(Utf8JsonWriter writer, LinkedList<T> value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();

        foreach (var item in value)
        {
            writer.WriteRawValue(JsonSerializer.Serialize(item, options));
        }
        
        writer.WriteEndArray();
    }
}