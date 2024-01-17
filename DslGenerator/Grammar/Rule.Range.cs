namespace RhoMicro.CodeAnalysis.DslGenerator.Grammar;

using System.Diagnostics;

#if DSL_GENERATOR
[IncludeFile]
#endif
abstract partial record Rule
{
    [DebuggerDisplay("{ToDisplayString()}")]
    public sealed partial record Range(Terminal Start, Terminal End) : Rule
    {
        public override String ToString() => base.ToString();
        public override void AppendDisplayStringTo(IndentedStringBuilder builder, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            _ = builder.AppendDisplayString(Start, cancellationToken).Append('-').AppendDisplayString(End, cancellationToken);
        }
        protected override void AppendCtorArgs(IndentedStringBuilder builder, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            _ = AppendCtorArg(AppendCtorArg(builder, nameof(Start), Start, cancellationToken).Append(", "), nameof(End), End, cancellationToken);
        }
    }
}
