namespace RhoMicro.CodeAnalysis.DslGenerator.Grammar;

using RhoMicro.CodeAnalysis.DslGenerator.Lexing;

using System.Diagnostics;
using System.Text;

#if DSL_GENERATOR
[IncludeFile]
#endif
[DebuggerDisplay("{ToDisplayString()}")]
abstract partial record Rule : SyntaxNode
{
    [DebuggerDisplay("{ToDisplayString()}")]
    public sealed record Alternative(Rule Left, Rule Right) : Rule
    {
        public override String ToString() => base.ToString();
        public override void AppendDisplayStringTo(StringBuilder builder) =>
            builder.AppendDisplayString(Left).Append(" / ").AppendDisplayString(Right);
        protected override void AppendCtorArgs(StringBuilder builder) =>
            AppendCtorArg(AppendCtorArg(builder, nameof(Left), Left).Append(", "), nameof(Right), Right);
    }
    [DebuggerDisplay("{ToDisplayString()}")]
    public sealed record Concatenation(Rule Left, Rule Right) : Rule
    {
        public override String ToString() => base.ToString();
        public override void AppendDisplayStringTo(StringBuilder builder) =>
            builder.AppendDisplayString(Left).Append(' ').AppendDisplayString(Right);
        protected override void AppendCtorArgs(StringBuilder builder) =>
            AppendCtorArg(AppendCtorArg(builder, nameof(Left), Left).Append(", "), nameof(Right), Right);
    }
    [DebuggerDisplay("{ToDisplayString()}")]
    public sealed record SpecificRepetition(Int32 Count, Rule Rule) : Rule
    {
        public override String ToString() => base.ToString();
        public override void AppendDisplayStringTo(StringBuilder builder) =>
            builder.Append(Count).AppendDisplayString(Rule);
        protected override void AppendCtorArgs(StringBuilder builder) =>
            AppendCtorArg(AppendCtorArg(builder, nameof(Count), Count.ToString()).Append(", "), nameof(Rule), Rule);
    }
    [DebuggerDisplay("{ToDisplayString()}")]
    public sealed record VariableRepetition(Rule Rule) : Rule
    {
        public override String ToString() => base.ToString();
        public override void AppendDisplayStringTo(StringBuilder builder) =>
            builder.Append('*').AppendDisplayString(Rule);
        protected override void AppendCtorArgs(StringBuilder builder) =>
            AppendCtorArg(builder, nameof(Rule), Rule);
    }
    [DebuggerDisplay("{ToDisplayString()}")]
    public sealed record Grouping(Rule Rule) : Rule
    {
        public override String ToString() => base.ToString();
        public override void AppendDisplayStringTo(StringBuilder builder) =>
            builder.Append('(').AppendDisplayString(Rule).Append(')');
        protected override void AppendCtorArgs(StringBuilder builder) =>
            AppendCtorArg(builder, nameof(Rule), Rule);
    }
    [DebuggerDisplay("{ToDisplayString()}")]
    public sealed record OptionalGrouping(Rule Rule) : Rule
    {
        public override String ToString() => base.ToString();
        public override void AppendDisplayStringTo(StringBuilder builder) =>
            builder.Append('[').AppendDisplayString(Rule).Append(']');
        protected override void AppendCtorArgs(StringBuilder builder) =>
            AppendCtorArg(builder, nameof(Rule), Rule);
    }
    [DebuggerDisplay("{ToDisplayString()}")]
    public sealed partial record Range(Terminal Start, Terminal End) : Rule
    {
        public override String ToString() => base.ToString();
        public override void AppendDisplayStringTo(StringBuilder builder) =>
            builder.AppendDisplayString(Start).Append('-').AppendDisplayString(End);
        protected override void AppendCtorArgs(StringBuilder builder) =>
            AppendCtorArg(AppendCtorArg(builder, nameof(Start), Start).Append(", "), nameof(End), End);
    }
    [DebuggerDisplay("{ToDisplayString()}")]
    public sealed partial record Terminal(String Value) : Rule
    {
        public override String ToString() => base.ToString();
        public Terminal(Token token)
            : this(token.Type == TokenType.Terminal ?
                  token.Lexeme.ToString() ?? String.Empty :
                  throw new ArgumentOutOfRangeException(nameof(token), token.Type, "Token must be of terminal type."))
        { }
        public override void AppendDisplayStringTo(StringBuilder builder) =>
            builder.Append('"').Append(Value).Append('"');
        protected override void AppendCtorArgs(StringBuilder builder) =>
            AppendCtorArg(builder, nameof(Value), Value, quoteValue: true);
        public Boolean Equals(Terminal other) => other is not null && Value == other.Value;
        public override Int32 GetHashCode() => Value.GetHashCode();
    }
    [DebuggerDisplay("{ToDisplayString()}")]
    public sealed partial record Reference(Name Name) : Rule
    {
        public override String ToString() => base.ToString();
        public override void AppendDisplayStringTo(StringBuilder builder) =>
            builder.AppendDisplayString(Name);
        protected override void AppendCtorArgs(StringBuilder builder) =>
            AppendCtorArg(builder, nameof(Name), Name);
    }
    [DebuggerDisplay("{ToDisplayString()}")]
    public sealed partial record Any : Rule
    {
        public static Any Instance { get; } = new();
        public override String ToString() => base.ToString();
        public override void AppendDisplayStringTo(StringBuilder builder) => builder.Append('.');
        protected override void AppendCtorArgs(StringBuilder builder) { }
    }
    public override String ToString() => base.ToString();
}
