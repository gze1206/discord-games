using Microsoft.CodeAnalysis;

namespace MessageCodeGen
{
    public static class Utils
    {
        internal static (string messageTypeName, string messageName) GetNames(ISymbol message)
        {
            var messageTypeName = message.Name;
            var messageName = messageTypeName.Substring(0, messageTypeName.Length - 7);

            return (messageTypeName, messageName);
        }
    }
}