namespace DiscordGames.Core.Net.Message;

public readonly partial record struct ErrorMessage
{
    public ResultCode ResultCode { get; init; }
}