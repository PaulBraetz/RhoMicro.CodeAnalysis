namespace RhoMicro.CodeAnalysis.DslGenerator.Grammar;

using System.Diagnostics;
using System.Linq;

#if DSL_GENERATOR
[IncludeFile]
#endif
[DebuggerDisplay("{ToDisplayString()}")]
record RuleList(IReadOnlyList<RuleDefinition> Definitions) : SyntaxNode
{
    public override String ToString() => base.ToString();
    public virtual Boolean Equals(RuleList other) =>
        other.Definitions.SequenceEqual(Definitions);
    public override Int32 GetHashCode() =>
        Definitions.Aggregate(997021164, (hc, def) => hc * -1521134295 + def.GetHashCode());
    public virtual void AppendCommentParagraphsTo(IndentedStringBuilder builder, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        _ = Definitions.Aggregate(builder, (b, d) =>
            b.Comment.OpenParagraph()
                .Comment.OpenCode()
                .AppendDisplayString(d, cancellationToken)
                .CloseBlock()
                .CloseBlock());
    }

    public override void AppendDisplayStringTo(IndentedStringBuilder builder, CancellationToken cancellationToken) =>
        _ = Definitions.Aggregate(builder, (sb, r) => sb.AppendDisplayString(r, cancellationToken).Append('\n'));
    protected override void AppendCtorArgs(IndentedStringBuilder builder, CancellationToken cancellationToken) =>
        AppendCtorArg(builder, nameof(Definitions), Definitions, cancellationToken);
}
