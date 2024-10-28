using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using CodeGenCore;
using Microsoft.CodeAnalysis;

using static MessageCodeGen.Utils;

namespace MessageCodeGen.Generators
{
    public static class MessageReadWriteGenerator
    {
        public static Task Generate(IReadOnlyList<INamedTypeSymbol> messages, CodeWriter writer)
        {
            if (messages == null) throw new ArgumentNullException(nameof(messages));
            if (writer == null) throw new ArgumentNullException(nameof(writer));
            
            writer.Write("using System;");
            writer.Write("using DiscordGames.Core.Memory;");
            writer.Write("using DiscordGames.Core.Net.Message;");
            writer.Write();
            writer.Write("namespace DiscordGames.Core.Net.Serialize;");
            writer.Write();
            writer.Write("public static partial class MessageSerializer");
            using (writer.BeginBlock())
            {
                GenRead(messages, writer);
                writer.Write();
                GenWrite(messages, writer);
            }
            
            return Task.CompletedTask;
        }

        private static void GenRead(IReadOnlyList<INamedTypeSymbol> messages, CodeWriter writer)
        {
            writer.Write("public static void Read(byte[] buffer, IMessageHandler handler)");
            using (writer.BeginBlock())
            {
                writer.Write("var payloadSize = buffer.Length - sizeof(int);");
                writer.Write("var checksum = BitConverter.ToUInt32(buffer.AsSpan(payloadSize));");
                writer.Write("var actualChecksum = CalcChecksum(buffer.AsSpan(0, payloadSize));");
                writer.Write("if (checksum != actualChecksum) ThrowHelper.ThrowChecksum();");
                writer.Write();
                writer.Write("var reader = new BufferReader(buffer);");
                writer.Write("var header = reader.ReadHeader();");
                writer.Write();
                writer.Write("switch (header.MessageType)");
                using (writer.BeginBlock())
                {
                    foreach (var message in messages)
                    {
                        var (typeName, name) = GetNames(message);
                        writer.Write($"case MessageType.{name}:");
                        using (writer.BeginBlock())
                        {
                            writer.Write($"handler.On{name}(reader.Read{typeName}(ref header)).GetAwaiter().GetResult();");
                            writer.Write("break;");
                        }
                    }
                    writer.Write("default: throw new NotImplementedException();");
                }
            }
        }

        private static void GenWrite(IReadOnlyList<INamedTypeSymbol> messages, CodeWriter writer)
        {
            foreach (var message in messages)
            {
                writer.Write($"public static byte[] Write(ref {message.Name} message)");
                using (writer.BeginBlock())
                {
                    writer.Write("var writer = new BufferWriter();");
                    writer.Write("writer.Write(message);");
                    writer.Write();
                    writer.Write("var size = writer.UsedTotal;");
                    writer.Write("var buffer = new byte[size + sizeof(int)];");
                    writer.Write("writer.CopyTo(buffer);");
                    writer.Write("writer.Dispose();");
                    writer.Write();
                    writer.Write("var checksum = CalcChecksum(buffer.AsSpan(0, size));");
                    writer.Write("BitConverter.TryWriteBytes(buffer.AsSpan(size), checksum);");
                    writer.Write("return buffer;");
                }
                writer.Write();

                var (typeName, name) = GetNames(message);
                var args = new StringBuilder("MessageChannel channel");
                var messageArgs = new StringBuilder("ref header");
                foreach (var property in GetMessageProperties(message))
                {
                    args.Append($", {property.Type.Name} {property.Name}");
                    messageArgs.Append($", {property.Name}");
                }
                
                writer.Write($"public static byte[] Write{typeName}({args})");
                using (writer.BeginBlock())
                {
                    writer.Write($"var header = new MessageHeader(SchemeVersion, channel, MessageType.{name});");
                    writer.Write($"var message = new {typeName}({messageArgs});");
                    writer.Write("return Write(ref message);");
                }
                writer.Write();
            }
        }
    }
}