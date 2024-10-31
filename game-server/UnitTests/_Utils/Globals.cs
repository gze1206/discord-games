using DiscordGames.Grain.Serialization;
using DiscordGames.Grain.Serialization.Json;

// ReSharper disable once CheckNamespace
namespace UnitTests.Utils;

public static class Globals
{
    public static CustomJsonGrainStorageSerializer Serializer()
        => new(
            SerializeDefines.JsonConvertBuilder
                .BakeOptions()
        );
}