using System;
using System.Linq;
using System.Xml.Linq;
using CodeGenCore;
using Microsoft.CodeAnalysis;

namespace ThrowHelperCodeGen
{
    [Generator]
    public class ThrowHelperGenerator : ISourceGenerator
    {
        private const string UsingRootName = "Usings";
        private const string UsingNodeName = "Using";
        private const string ExceptionRootName = "Exceptions";
        private const string ExceptionNodeName = "Exception";
        
        public void Initialize(GeneratorInitializationContext context) { }

        public void Execute(GeneratorExecutionContext context)
        {
            var xmlFile = context.AdditionalFiles.FirstOrDefault(x =>
                x.Path.EndsWith("ThrowHelper.xml", StringComparison.OrdinalIgnoreCase));
            if (xmlFile == null) return;

            var xml = xmlFile.GetText(context.CancellationToken)?.ToString();
            if (string.IsNullOrWhiteSpace(xml)) return;
            
            var doc = XDocument.Parse(xml).Root;
            var writer = new CodeWriter();

            writer.Write("using System.Runtime.CompilerServices;");
            foreach (var node in doc.Descendants(UsingRootName).SelectMany(x => x.Descendants(UsingNodeName)))
            {
                var value = node.Value;
                if (string.IsNullOrWhiteSpace(value)) continue;
                
                writer.Write($"using {value};");
            }
            writer.Write();

            var classNamePart = doc.Attribute("Name")?.Value ?? string.Empty;
            
            writer.Write($"public static class {classNamePart}ThrowHelper");
            using (writer.BeginBlock())
            {
                foreach (var node in doc.Descendants(ExceptionRootName).SelectMany(x => x.Descendants(ExceptionNodeName)))
                {
                    var type = node.Attribute("Type")?.Value ?? "(Require Specify Type)";
                    var name = node.Attribute("Name")?.Value ?? type.Replace("Exception", "");
                    var message = node.Attribute("Message")?.Value ?? string.Empty;
                    
                    writer.Write($"private static readonly Lazy<{type}> {name}Instance = new Lazy<{type}>(() => new {type}(\"{message}\"));");
                    writer.Write($"[MethodImpl(MethodImplOptions.AggressiveInlining)] public static void Throw{name}() => throw {name}Instance.Value;");
                    writer.Write();
                }
            }
            
            context.AddSource("ThrowHelper.g.cs", writer.ToSourceText());
        }
    }
}