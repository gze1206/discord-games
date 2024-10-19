using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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
            writer.Write("public static void Read(ReadOnlySpan<byte> data, IMessageHandler handler)");
            using (writer.BeginBlock())
            {
                writer.Write("var version = (byte)((data[0] & VersionMask) >> VersionBits);");
                writer.Write("if (version != SchemeVersion) throw MessageSchemeVersionException.I;");
                writer.Write();
                writer.Write("var channel = (MessageChannel)(data[0] & ~VersionMask);");
                writer.Write("var type = (MessageType)data[1];");
                writer.Write("var checksum = BitConverter.ToUInt32(data[2..6]);");
                writer.Write();
                writer.Write("const int payloadBeginAt = MessageHeader.HeaderSize;");
                writer.Write("var actualChecksum = CalcChecksum(data[payloadBeginAt..]);");
                writer.Write("if (checksum != actualChecksum) throw InvalidMessageChecksumException.I;");
                writer.Write();
                writer.Write("var header = new MessageHeader(version, channel, type);");
                writer.Write();
                writer.Write("switch (type)");
                using (writer.BeginBlock())
                {
                    foreach (var message in messages)
                    {
                        WriteReaderBody(message, writer);
                    }
                    
                    writer.Write("default: throw new NotImplementedException();");
                }
            }
        }
        
        return Task.CompletedTask;
    }

    private static void WriteReaderBody(INamedTypeSymbol message, CodeWriter writer)
    {
        var (messageTypeName, messageName) = GetNames(message);
        writer.Write($"case MessageType.{messageName}:");
        using (writer.BeginBlock())
        {
            var parameters = message.Constructors.First().Parameters;
            var args = new List<string>(parameters.Length);
            var offset = 0;
            
            for (var i = 0; i < parameters.Length; i++)
            {
                var param = parameters[i];
                var paramType = param.Type.Name;
                var argName = $"_{i}";
                var start = offset.ToString(CultureInfo.InvariantCulture);
                var size = SizeOf(paramType);
                var end = "null";

                if (size.HasValue)
                {
                    offset += size.Value;
                    end = offset.ToString(CultureInfo.InvariantCulture);
                }
                
                writer.Write($"/* {param.Name} */ var {argName} = BitConverter.To{paramType}(data[(payloadBeginAt + {start})..(payloadBeginAt + {end})]);");
                args.Add(argName);
            }

            var argsCode = string.Join(", ", args);
            writer.Write($"handler.On{messageName}(new {messageTypeName}({argsCode}) {{ Header = header }});");
            writer.Write("break;");
        }
    }
}
