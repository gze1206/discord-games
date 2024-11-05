using DiscordGames.Grains.Serialization;
using DiscordGames.Grains.Serialization.Json;

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