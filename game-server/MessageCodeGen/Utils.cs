using System.Collections.Generic;
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

        internal static IEnumerable<IPropertySymbol> GetMessageProperties(INamedTypeSymbol message)
        {
            foreach (var property in message.GetMembers())
            {
                if (property is not IPropertySymbol propertySymbol) continue;
                if (propertySymbol.SetMethod?.IsInitOnly != true) continue;

                yield return propertySymbol;
            }
        }
    }
}