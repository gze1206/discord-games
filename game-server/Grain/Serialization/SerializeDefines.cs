using DiscordGames.Grain.Serialization.Json;

namespace DiscordGames.Grain.Serialization;

public static class SerializeDefines
{
    public static readonly CustomJsonConvertBuilder JsonConvertBuilder = new CustomJsonConvertBuilder()
        .Add<LinkedListJsonConverter<int>>();
}