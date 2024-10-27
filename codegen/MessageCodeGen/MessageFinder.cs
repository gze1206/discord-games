using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

// ReSharper disable once CheckNamespace
public class MessageFinder : ISyntaxReceiver
{
    public List<RecordDeclarationSyntax> Messages { get; } = new List<RecordDeclarationSyntax>();

    public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
    {
        if (syntaxNode is not RecordDeclarationSyntax message) return;
        if (!message.Identifier.ValueText.EndsWith("Message", StringComparison.OrdinalIgnoreCase)) return;
            
        this.Messages.Add(message);
    }
}
