namespace RhoMicro.CodeAnalysis.DslGenerator.Grammar;

using RhoMicro.CodeAnalysis.DslGenerator.Lexing;

using System.Diagnostics;
using System.Text;

#if DSL_GENERATOR
[IncludeFile]
#endif
[DebuggerDisplay("{ToDisplayString()}")]
sealed partial record Name : SyntaxNode
{
    public Name(Token token) : this(token.Lexeme) { }
    public Name(Lexeme lexeme) : this(lexeme.ToString() ?? String.Empty) { }
#pragma warning disable IDE1006 // Naming Styles
    public Name(String Value)
#pragma warning restore IDE1006 // Naming Styles
    {
        for(var i = 0; i < Value.Length; i++)
        {
            if(!Utils.IsValidNameChar(Value[i]))
            {
                throw new ArgumentException($"Invalid character at index {i}: '{Value[i]}'. Rule names must contain letters only.)", nameof(Value));
            }
        }

        this.Value = Value;
    }

    public override String ToString() => base.ToString();
    public String Value { get; }
    public override void AppendDisplayStringTo(StringBuilder builder) => builder.Append(Value);
    protected override void AppendCtorArgs(StringBuilder builder) => AppendCtorArg(builder, nameof(Value), Value, quoteValue: true);
}
