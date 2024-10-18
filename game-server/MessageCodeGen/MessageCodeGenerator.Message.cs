using System.Collections.Generic;
using System.Threading.Tasks;
using MessageCodeGen;
using Microsoft.CodeAnalysis;

// ReSharper disable once CheckNamespace
public partial class MessageCodeGenerator
{
    private static Task GenMessagePartial(List<INamedTypeSymbol> messages, CodeWriter writer)
    {
        writer.Write("namespace DiscordGames.Core.Net.Message;");
        writer.Write();

        foreach (var message in messages)
        {
            writer.Write($"public partial record struct {message.Name}");
            using (writer.BeginBlock())
            {
                var totalSize = 0;
                var hasNotImplemented = false;
                var info = new List<string>();
                
                foreach (var property in message.GetMembers())
                {
                    if (property is not IPropertySymbol propertySymbol) continue;
                    if (propertySymbol.SetMethod?.IsInitOnly != true) continue;

                    var size = SizeOf(propertySymbol.Type.Name);
                    info.Add($"{propertySymbol.Name}({propertySymbol.Type.Name} - {size ?? -1})");
                    
                    if (size.HasValue) totalSize += size.Value;
                    else hasNotImplemented = true;
                }
                
                writer.Write($"// {string.Join(" + ", info)}");
                writer.Write($"internal const int PayloadSize = {(hasNotImplemented ? "null" : totalSize)};");
                writer.Write();
                writer.Write("public MessageHeader Header { get; init; }");
            }
        }

        return Task.CompletedTask;
    }
}
