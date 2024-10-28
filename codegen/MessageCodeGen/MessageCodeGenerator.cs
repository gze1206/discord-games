using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CodeGenCore;
using MessageCodeGen.Generators;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace MessageCodeGen
{
    [Generator]
    public class MessageCodeGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new MessageFinder());
        }
    
        public void Execute(GeneratorExecutionContext context)
        {
            var messages = (context.SyntaxReceiver as MessageFinder)?.Messages
                           .Select(x => context.Compilation
                               .GetSemanticModel(x.SyntaxTree)
                               .GetDeclaredSymbol(x)!)
                           .ToList()
                           ?? new List<INamedTypeSymbol>();

            var messagePartials = new CodeWriter();
            var handler = new CodeWriter();
            var bufferReadWrite = new CodeWriter();
            var readWrite = new CodeWriter();
            var type = new CodeWriter();

            Task.WaitAll(
                MessageHandlerGenerator.Generate(messages, handler),
                MessagePartialGenerator.Generate(messages, messagePartials),
                MessageBufferReadWriteGenerator.Generate(messages, bufferReadWrite),
                MessageReadWriteGenerator.Generate(messages, readWrite),
                MessageTypeGenerator.Generate(messages, type)
            );

            context.AddSource("Message.g.cs", messagePartials.ToSourceText());
            context.AddSource("MessageHandler.g.cs", handler.ToSourceText());
            context.AddSource("MessageSerializer.Buffer.g.cs", bufferReadWrite.ToSourceText());
            context.AddSource("MessageSerializer.ReadWrite.g.cs", readWrite.ToSourceText());
            context.AddSource("MessageType.g.cs", type.ToSourceText());
        }
    }
}
