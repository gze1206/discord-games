using DiscordGames.Grains.Serialization.Json;

namespace DiscordGames.Grains.Serialization;

public static class SerializeDefines
{
    public static readonly CustomJsonConvertBuilder JsonConvertBuilder = new CustomJsonConvertBuilder()
        .Add<LinkedListJsonConverter<int>>();
}