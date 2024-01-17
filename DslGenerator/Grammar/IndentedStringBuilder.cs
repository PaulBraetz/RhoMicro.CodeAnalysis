namespace RhoMicro.CodeAnalysis.Library.Text;

using RhoMicro.CodeAnalysis.DslGenerator.Grammar;

#if DSL_GENERATOR
[IncludeFile]
#endif
partial class IndentedStringBuilder
{
    public IndentedStringBuilder AppendCommentParagraphs(RuleList ruleList, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        ruleList.AppendCommentParagraphsTo(this, cancellationToken);

        return this;
    }
    public IndentedStringBuilder AppendDisplayString(SyntaxNode node, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        node.AppendDisplayStringTo(this, cancellationToken);

        return this;
    }
    public IndentedStringBuilder AppendMetaString(SyntaxNode node, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        node.AppendMetaStringTo(this, cancellationToken);

        return this;
    }
}
