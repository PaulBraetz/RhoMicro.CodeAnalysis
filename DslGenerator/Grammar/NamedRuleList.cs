namespace RhoMicro.CodeAnalysis.DslGenerator.Grammar;

using System;
using System.Collections.Generic;
using System.Diagnostics;

#if DSL_GENERATOR
[IncludeFile]
#endif
[DebuggerDisplay("{ToDisplayString()}")]
record NamedRuleList(Name Name, IReadOnlyList<RuleDefinition> Definitions) : RuleList(Definitions)
{
    public override String ToString() => base.ToString();
    public virtual Boolean Equals(NamedRuleList other) => Name.Equals(other.Name) && base.Equals(other);
    public override Int32 GetHashCode() => base.GetHashCode() * -1521134295 + Name.GetHashCode();
    public override void AppendCommentParagraphsTo(IndentedStringBuilder builder, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        _ = builder.Comment.OpenParagraph()
            .Append("Name: ")
            .Comment.OpenCode()
            .AppendDisplayString(Name, cancellationToken)
            .CloseBlock()
            .CloseBlock();

        base.AppendCommentParagraphsTo(builder, cancellationToken);
    }
    public override void AppendDisplayStringTo(IndentedStringBuilder builder, CancellationToken cancellationToken) =>
        base.AppendDisplayStringTo(builder.AppendDisplayString(Name, cancellationToken).AppendLine(";"), cancellationToken);
    protected override void AppendCtorArgs(IndentedStringBuilder builder, CancellationToken cancellationToken) =>
        AppendCtorArg(AppendCtorArg(builder, nameof(Name), Name, cancellationToken).Append(", "), nameof(Definitions), Definitions, cancellationToken);
    public override void Receive(SyntaxNodeVisitor visitor) => visitor.Visit(this);
}
