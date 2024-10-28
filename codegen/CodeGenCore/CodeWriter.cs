using System;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Microsoft.CodeAnalysis.Text;

namespace CodeGenCore
{
    [SuppressMessage("Design", "CA1034:중첩 형식을 표시하지 않아야 합니다.")]
    [SuppressMessage("Performance", "CA1815:값 형식에서 Equals 또는 같음 연산자를 재정의하세요.")]
    public class CodeWriter
    {
        private readonly StringBuilder builder;

        private int indentLevel;

        public override string ToString() => this.builder.ToString();

        public CodeWriter()
        {
            this.builder = new();
            this.indentLevel = 0;
        }
        
        public SourceText ToSourceText() => SourceText.From(this.builder.ToString(), Encoding.UTF8);

        public void Write(string line)
        {
            this.builder.Append('\t', this.indentLevel);
            this.builder.AppendLine(line);
        }

        public void Write()
        {
            this.builder.AppendLine();
        }

        public Indent BeginIndent()
            => new Indent(this);

        public Block BeginBlock(bool withTrailSemicolon = false)
            => new Block(this, withTrailSemicolon);

        public readonly struct Block : IDisposable
        {
            private readonly CodeWriter writer;
            private readonly bool withTrailSemicolon;
                
            internal Block(CodeWriter writer, bool withTrailSemicolon = false)
            {
                this.writer = writer;
                this.withTrailSemicolon = withTrailSemicolon;
                    
                this.writer.Write("{");
                this.writer.indentLevel++;
            }
                
            public void Dispose()
            {
                this.writer.indentLevel--;
                this.writer.Write(this.withTrailSemicolon ? "};" : "}");
            }
        }

        public readonly struct Indent : IDisposable
        {
            private readonly CodeWriter writer;

            internal Indent(CodeWriter writer)
            {
                this.writer = writer;
                this.writer.indentLevel++;
            }

            public void Dispose()
            {
                this.writer.indentLevel--;
            }
        }
    }
}

