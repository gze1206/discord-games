using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CodeGenCore;
using Microsoft.CodeAnalysis;

using static MessageCodeGen.Utils;

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
                
                    foreach (var property in GetMessageProperties(message))
                    {
                        args.Add($"{property.Type.Name} {property.Name}");
                        body.Add($"this.{property.Name} = {property.Name};");
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