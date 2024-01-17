namespace RhoMicro.CodeAnalysis.DslGenerator.Grammar;

using RhoMicro.CodeAnalysis.DslGenerator.Lexing;

using System.Diagnostics;

#if DSL_GENERATOR
[IncludeFile]
#endif
abstract partial record Rule
{
    [DebuggerDisplay("{ToDisplayString()}")]
    public sealed partial record Terminal(String Value) : Rule
    {
        public override String ToString() => base.ToString();
        public Terminal(Token token)
            : this(token.Type == TokenType.Terminal ?
                  token.Lexeme.ToString() ?? String.Empty :
                  throw new ArgumentOutOfRangeException(nameof(token), token.Type, "Token must be of terminal type."))
        { }
        public override void AppendDisplayStringTo(IndentedStringBuilder builder, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            _ = builder.Append('"').Append(Value).Append('"');
        }

        protected override void AppendCtorArgs(IndentedStringBuilder builder, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            
            _ = AppendCtorArg(builder, nameof(Value), Value, quoteValue: true, cancellationToken);
        }
        public Boolean Equals(Terminal other) => other is not null && Value == other.Value;
        public override Int32 GetHashCode() => Value.GetHashCode();
    }
}
