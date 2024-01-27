namespace RhoMicro.CodeAnalysis.DslGenerator.Grammar;

using System.Diagnostics;

#if DSL_GENERATOR
[IncludeFile]
#endif
abstract partial record Rule
{
    [DebuggerDisplay("{ToDisplayString()}")]
    public sealed partial record Reference(Name Name) : Rule
    {
        public override String ToString() => base.ToString();
        public override void AppendDisplayStringTo(IndentedStringBuilder builder, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            _ = builder.AppendDisplayString(Name, cancellationToken);
        }

        protected override void AppendCtorArgs(IndentedStringBuilder builder, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            _ = AppendCtorArg(builder, nameof(Name), Name, cancellationToken);
        }
        public override void Receive(SyntaxNodeVisitor visitor) => visitor.Visit(this);
    }
}
