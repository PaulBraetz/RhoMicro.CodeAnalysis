namespace RhoMicro.CodeAnalysis.DslGenerator.Grammar;

using System.Diagnostics;

#if DSL_GENERATOR
[IncludeFile]
#endif
abstract partial record Rule
{
    [DebuggerDisplay("{ToDisplayString()}")]
    public sealed record SpecificRepetition(Int32 Count, Rule Rule) : Rule
    {
        public override String ToString() => base.ToString();
        public override void AppendDisplayStringTo(IndentedStringBuilder builder, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            _ = builder.Append(Count.ToString()).AppendDisplayString(Rule, cancellationToken);
        }
        protected override void AppendCtorArgs(IndentedStringBuilder builder, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            _ = AppendCtorArg(
                AppendCtorArg(builder, nameof(Count), Count.ToString(), quoteValue: false, cancellationToken)
                .Append(", "), nameof(Rule), Rule, cancellationToken);
        }
        public override void Receive(SyntaxNodeVisitor visitor) => visitor.Visit(this);
    }
}
