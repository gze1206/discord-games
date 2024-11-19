using System.Text.Json.Serialization;

namespace DiscordGames.Grains.States;

[GenerateSerializer]
[Alias("DiscordGames.Grains.States.UserState")]
public class UserState
{
    [Id(0)] public bool IsConnected { get; internal set; }
    [Id(1)] public string? PlayingSessionId { get; internal set; }
    
    public UserState() { }

    [JsonConstructor]
    public UserState(
        bool isConnected,
        string? playingSessionId
    )
    {
        this.IsConnected = isConnected;
        this.PlayingSessionId = playingSessionId;
    }
}