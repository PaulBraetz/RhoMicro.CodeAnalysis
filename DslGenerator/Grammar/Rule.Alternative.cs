namespace RhoMicro.CodeAnalysis.DslGenerator.Grammar;

using System.Diagnostics;

#if DSL_GENERATOR
[IncludeFile]
#endif
abstract partial record Rule
{
    [DebuggerDisplay("{ToDisplayString()}")]
    public sealed record Alternative(Rule Left, Rule Right) : Rule
    {
        public override String ToString() => base.ToString();
        public override void AppendDisplayStringTo(IndentedStringBuilder builder, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            _ = builder.AppendDisplayString(Left, cancellationToken).Append(" / ").AppendDisplayString(Right, cancellationToken);
        }

        protected override void AppendCtorArgs(IndentedStringBuilder builder, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            _ = AppendCtorArg(
                AppendCtorArg(builder, nameof(Left), Left, cancellationToken)
                .Append(", "), nameof(Right), Right, cancellationToken);
        }
        public override void Receive(SyntaxNodeVisitor visitor) => visitor.Visit(this);
    }
}
