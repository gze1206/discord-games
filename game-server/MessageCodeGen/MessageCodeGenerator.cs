using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MessageCodeGen;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

[Generator]
// ReSharper disable once CheckNamespace
public partial class MessageCodeGenerator : ISourceGenerator
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
        var serializerRead = new CodeWriter();
        var serializerWrite = new CodeWriter();

        Task.WaitAll(
            GenMessagePartial(messages, messagePartials),
            GenMessageHandler(messages, handler),
            GenMessageReader(messages, serializerRead),
            GenMessageWriter(messages, serializerWrite)
        );

        context.AddSource("Message.g.cs", SourceText.From(messagePartials.ToString(), Encoding.UTF8));
        context.AddSource("MessageHandler.g.cs", SourceText.From(handler.ToString(), Encoding.UTF8));
        context.AddSource("MessageSerializer.Read.g.cs", SourceText.From(serializerRead.ToString(), Encoding.UTF8));
        context.AddSource("MessageSerializer.Write.g.cs", SourceText.From(serializerWrite.ToString(), Encoding.UTF8));
    }

    private static (string messageTypeName, string messageName) GetNames(ISymbol message)
    {
        var messageTypeName = message.Name;
        var messageName = messageTypeName.Substring(0, messageTypeName.Length - 7);

        return (messageTypeName, messageName);
    }

    private static int? SizeOf(string typeName) => typeName switch
    {
        "Int32" => sizeof(int),
        "Int64" => sizeof(long),
        _ => null
    };
}
