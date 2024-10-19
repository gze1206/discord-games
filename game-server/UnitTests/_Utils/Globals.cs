using DiscordGames.Server.Serialization.Json;

// ReSharper disable once CheckNamespace
namespace UnitTests.Utils;

public static class Globals
{
    public static CustomJsonGrainStorageSerializer Serializer()
        => new(
            new CustomJsonConvertBuilder()
                .Add<LinkedListJsonConverter<int>>()
                .BakeOptions()
        );
}