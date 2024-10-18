using System.Collections.Generic;
using System.Threading.Tasks;
using MessageCodeGen;
using Microsoft.CodeAnalysis;

// ReSharper disable once CheckNamespace
public partial class MessageCodeGenerator
{
    private static Task GenMessageHandler(List<INamedTypeSymbol> messages, CodeWriter writer)
    {
        writer.Write("using DiscordGames.Core.Net.Message;");
        writer.Write();
        writer.Write("namespace DiscordGames.Core.Net;");
        writer.Write();
        writer.Write("public partial interface IMessageHandler");
        using (writer.BeginBlock())
        {
            foreach (var message in messages)
            {
                var (messageTypeName, messageName) = GetNames(message);
                writer.Write($"Task On{messageName}({messageTypeName} message);");
            }
        }
        
        return Task.CompletedTask;
    }
}
