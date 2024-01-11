namespace RhoMicro.CodeAnalysis.DslGenerator.Grammar;

using System.Diagnostics;
using System.Text;

#if DSL_GENERATOR
[IncludeFile]
#endif
[DebuggerDisplay("{ToDisplayString()}")]
abstract partial record RuleDefinition(Name Name, Rule Rule) : SyntaxNode
{
    [DebuggerDisplay("{ToDisplayString()}")]
    public sealed record New : RuleDefinition
    {
#pragma warning disable IDE1006 // Naming Styles
        public New(Name Name, Rule Rule) : base(Name, Rule) { }
#pragma warning restore IDE1006 // Naming Styles
        public override String ToString() => base.ToString();
        public override void AppendDisplayStringTo(StringBuilder builder) =>
            builder.AppendDisplayString(Name).Append(" = ").AppendDisplayString(Rule).Append(';');
    }
    [DebuggerDisplay("{ToDisplayString()}")]
    public sealed record Incremental : RuleDefinition
    {
#pragma warning disable IDE1006 // Naming Styles
        public Incremental(Name Name, Rule Rule) : base(Name, Rule) { }
#pragma warning restore IDE1006 // Naming Styles
        public override String ToString() => base.ToString();
        public override void AppendDisplayStringTo(StringBuilder builder) =>
            builder.AppendDisplayString(Name).Append(" /= ").AppendDisplayString(Rule).Append(';');
    }
    public override String ToString() => base.ToString();
    protected override void AppendCtorArgs(StringBuilder builder) =>
        AppendCtorArg(AppendCtorArg(builder, nameof(Name), Name).Append(", "), nameof(Rule), Rule);
}
