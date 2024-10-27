using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CodeGenCore;
using Microsoft.CodeAnalysis;

using static MessageCodeGen.Utils;

namespace MessageCodeGen.Generators
{
    public static class MessageTypeGenerator
    {
        public static Task Generate(IReadOnlyList<INamedTypeSymbol> messages, CodeWriter writer)
        {
            if (messages == null) throw new ArgumentNullException(nameof(messages));
            if (writer == null) throw new ArgumentNullException(nameof(writer));
            
            writer.Write("namespace DiscordGames.Core.Net;");
            writer.Write();
            
            writer.Write("public enum MessageType : byte");
            using (writer.BeginBlock())
            {
                foreach (var message in messages)
                {
                    var (_, name) = GetNames(message);
                    writer.Write($"{name},");
                }
            }

            return Task.CompletedTask;
        }
    }
}