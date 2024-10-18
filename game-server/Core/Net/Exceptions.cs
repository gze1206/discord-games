namespace DiscordGames.Core.Net;

public class MessageSchemeVersionException : ExceptionWithInstance<MessageSchemeVersionException>
{
    public MessageSchemeVersionException() : base("메시지의 SchemeVersion이 유효하지 않습니다.")
    { }
}

public class InvalidMessageChecksumException : ExceptionWithInstance<InvalidMessageChecksumException>
{
    public InvalidMessageChecksumException() : base("메시지의 Checksum이 유효하지 않습니다.")
    { }
}