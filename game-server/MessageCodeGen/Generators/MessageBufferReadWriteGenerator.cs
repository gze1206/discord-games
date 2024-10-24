using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;

namespace MessageCodeGen.Generators
{
    public static class MessageBufferReadWriteGenerator
    {
        public static Task Generate(IReadOnlyList<INamedTypeSymbol> messages, CodeWriter writer)
        {
            if (messages == null) throw new ArgumentNullException(nameof(messages));
            if (writer == null) throw new ArgumentNullException(nameof(writer));
            
            writer.Write("using DiscordGames.Core.Net;");
            writer.Write("using DiscordGames.Core.Net.Message;");
            writer.Write("using DiscordGames.Core.Memory;");
            writer.Write();
            writer.Write("namespace DiscordGames.Core.Net.Serialize;");
            writer.Write();
            writer.Write("public static partial class MessageSerializer");
            using (writer.BeginBlock())
            {
                foreach (var message in messages)
                {
                    GenRead(message, writer);
                    writer.Write();
                    GenWrite(message, writer);
                    writer.Write();
                }
            }
            
            return Task.CompletedTask;
        }

        private static void GenRead(INamedTypeSymbol message, CodeWriter writer)
        {
            var args = new StringBuilder("ref header");
            var body = new List<string>();
            var index = 0;
            
            foreach (var property in message.GetMembers())
            {
                if (property is not IPropertySymbol propertySymbol) continue;
                if (propertySymbol.SetMethod?.IsInitOnly != true) continue;

                args.Append($", _{index}");
                body.Add($"var _{index} = reader.Read{propertySymbol.Type.Name}();");
                index++;
            }
            
            writer.Write($"public static {message.Name} Read{message.Name}(this ref BufferReader reader, ref MessageHeader header)");
            using (writer.BeginBlock())
            {
                foreach (var line in body)
                {
                    writer.Write(line);
                }
                writer.Write($"return new {message.Name}({args});");
            }
            writer.Write();
            writer.Write($"public static {message.Name} Read{message.Name}(this ref BufferReader reader)");
            using (writer.BeginBlock())
            {
                writer.Write("var header = reader.ReadHeader();");
                foreach (var line in body)
                {
                    writer.Write(line);
                }
                writer.Write($"return new {message.Name}({args});");
            }
        }

        private static void GenWrite(INamedTypeSymbol message, CodeWriter writer)
        {
            writer.Write($"public static bool Write(this ref BufferWriter writer, {message.Name} message)");
            using (writer.BeginBlock())
            {
                writer.Write("var succeed = true;");
                writer.Write("succeed &= writer.Write(message.Header);");
                foreach (var property in message.GetMembers())
                {
                    if (property is not IPropertySymbol propertySymbol) continue;
                    if (propertySymbol.SetMethod?.IsInitOnly != true) continue;

                    writer.Write($"succeed &= writer.Write(message.{propertySymbol.Name});");
                }
                writer.Write("return succeed;");
            }
        }
    }
}