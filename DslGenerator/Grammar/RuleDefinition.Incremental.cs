namespace RhoMicro.CodeAnalysis.DslGenerator.Grammar;

using System.Diagnostics;

#if DSL_GENERATOR
[IncludeFile]
#endif
abstract partial record RuleDefinition
{
    [DebuggerDisplay("{ToDisplayString()}")]
    public sealed record Incremental : RuleDefinition
    {
#pragma warning disable IDE1006 // Naming Styles
        public Incremental(Name Name, Rule Rule) : base(Name, Rule) { }
#pragma warning restore IDE1006 // Naming Styles
        public override String ToString() => base.ToString();
        public override void AppendDisplayStringTo(IndentedStringBuilder builder, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            _ = builder.AppendDisplayString(Name, cancellationToken).Append(" /= ").AppendDisplayString(Rule, cancellationToken).Append(';');
        }
        public override void Receive(SyntaxNodeVisitor visitor) => visitor.Visit(this);
    }
}
