using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace MessageCodeGen.Generators
{
    public static class MessagePartialGenerator
    {
        public static Task Generate(IReadOnlyList<INamedTypeSymbol> messages, CodeWriter writer)
        {
            if (messages == null) throw new ArgumentNullException(nameof(messages));
            if (writer == null) throw new ArgumentNullException(nameof(writer));

            writer.Write("using System;");
            writer.Write("using System.Runtime.InteropServices;");
            writer.Write();
            writer.Write("namespace DiscordGames.Core.Net.Message;");
            writer.Write();

            foreach (var message in messages)
            {
                writer.Write("[StructLayout(LayoutKind.Auto)]");
                writer.Write($"public partial record struct {message.Name}");
                using (writer.BeginBlock())
                {
                    var args = new List<string> { "ref MessageHeader header" };
                    var body = new List<string> { "this.Header = header;" };
                
                    foreach (var property in message.GetMembers())
                    {
                        if (property is not IPropertySymbol propertySymbol) continue;
                        if (propertySymbol.SetMethod?.IsInitOnly != true) continue;

                        args.Add($"{propertySymbol.Type.Name} {propertySymbol.Name}");
                        body.Add($"this.{propertySymbol.Name} = {propertySymbol.Name};");
                    }
                    
                    writer.Write("public MessageHeader Header { get; }");
                    writer.Write();
                    writer.Write($"public {message.Name}({string.Join(", ", args)})");
                    using (writer.BeginBlock())
                    {
                        foreach (var line in body)
                        {
                            writer.Write(line);
                        }
                    }
                }
                writer.Write();
            }
            
            return Task.CompletedTask;
        }
    }
}