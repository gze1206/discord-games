using System.Collections.Generic;
using System.Threading.Tasks;
using MessageCodeGen;
using Microsoft.CodeAnalysis;

// ReSharper disable once CheckNamespace
public partial class MessageCodeGenerator
{
    private static Task GenMessageReader(List<INamedTypeSymbol> messages, CodeWriter writer)
    {
        writer.Write("using DiscordGames.Core.Net.Message;");
        writer.Write();
        writer.Write("namespace DiscordGames.Core.Net;");
        writer.Write();
        writer.Write("public static partial class MessageSerializer");
        using (writer.BeginBlock())
        {
            
        }
        
        return Task.CompletedTask;
    }
}
