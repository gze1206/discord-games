using System;

namespace DiscordGames.Core.Net;

public abstract class MessageException : Exception
{
    protected MessageException(string message) : base(message) { }
}

public class MessageSchemeVersionException : MessageException
{
    public MessageSchemeVersionException(string message) : base(message) { }
}

public class MessageChecksumException : MessageException
{
    public MessageChecksumException(string message) : base(message) { }
}
