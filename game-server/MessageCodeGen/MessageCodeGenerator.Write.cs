using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using MessageCodeGen;
using Microsoft.CodeAnalysis;

// ReSharper disable once CheckNamespace
public partial class MessageCodeGenerator
{
    private static Task GenMessageWriter(List<INamedTypeSymbol> messages, CodeWriter writer)
    {
        writer.Write("using DiscordGames.Core.Net.Message;");
        writer.Write();
        writer.Write("namespace DiscordGames.Core.Net;");
        writer.Write();
        writer.Write("public static partial class MessageSerializer");
        using (writer.BeginBlock())
        {
            foreach (var message in messages)
            {
                var (messageTypeName, messageName) = GetNames(message);
                writer.Write($"public static byte[] Write(this {messageTypeName} message)");
                using (writer.BeginBlock())
                {
                    writer.Write($"var buffer = new Span<byte>(new byte[MessageHeader.HeaderSize + {messageTypeName}.PayloadSize]);");
                    writer.Write();
                    writer.Write("// Header (Except checksum)");
                    writer.Write("buffer[0] |= (byte)(VersionMask & (message.Header.SchemeVersion << VersionBits));");
                    writer.Write("buffer[0] |= (byte)(~VersionMask & (byte)message.Header.Channel);");
                    writer.Write("buffer[1] = (byte)message.Header.MessageType;");
                    writer.Write();
                    writer.Write("const int payloadBeginAt = MessageHeader.HeaderSize;");
                    writer.Write();
                    writer.Write("// Payload");

                    var offset = 0;

                    foreach (var param in message.Constructors.First().Parameters)
                    {
                        var paramType = param.Type.Name;
                        var start = offset.ToString(CultureInfo.InvariantCulture);
                        var size = SizeOf(paramType);
                        var end = "null";

                        if (size.HasValue)
                        {
                            offset += size.Value;
                            end = offset.ToString(CultureInfo.InvariantCulture);
                        }

                        writer.Write($"BitConverter.TryWriteBytes(buffer[(payloadBeginAt + {start})..(payloadBeginAt + {end})], message.{param.Name});");
                    }
                    
                    writer.Write();
                    writer.Write("// Checksum");
                    writer.Write("BitConverter.TryWriteBytes(buffer[2..6], CalcChecksum(buffer[payloadBeginAt..]));");
                    writer.Write();
                    writer.Write("return buffer.ToArray();");
                }
                writer.Write();
            }
        }
        
        return Task.CompletedTask;
    }
}
