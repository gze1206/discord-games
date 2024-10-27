using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodeGenCore;
using MessageCodeGen.Generators;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

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

            context.AddSource("Message.g.cs", SourceText.From(messagePartials.ToString(), Encoding.UTF8));
            context.AddSource("MessageHandler.g.cs", SourceText.From(handler.ToString(), Encoding.UTF8));
            context.AddSource("MessageSerializer.Buffer.g.cs", SourceText.From(bufferReadWrite.ToString(), Encoding.UTF8));
            context.AddSource("MessageSerializer.ReadWrite.g.cs", SourceText.From(readWrite.ToString(), Encoding.UTF8));
            context.AddSource("MessageType.g.cs", SourceText.From(type.ToString(), Encoding.UTF8));
        }
    }
}
