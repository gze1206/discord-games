using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using static MessageCodeGen.Utils;

namespace MessageCodeGen.Generators
{
    public static class MessageHandlerGenerator
    {
        public static Task Generate(IReadOnlyList<INamedTypeSymbol> messages, CodeWriter writer)
        {
            if (messages == null) throw new ArgumentNullException(nameof(messages));
            if (writer == null) throw new ArgumentNullException(nameof(writer));
            
            writer.Write("using System.Threading.Tasks;");
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
}