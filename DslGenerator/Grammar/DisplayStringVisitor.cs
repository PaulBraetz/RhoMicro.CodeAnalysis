namespace RhoMicro.CodeAnalysis.DslGenerator.Grammar;
using System;

sealed class DisplayStringVisitor(IndentedStringBuilder builder) : SyntaxNodeVisitor
{
    public DisplayStringVisitor() : this(new()) { }
    private readonly IndentedStringBuilder _builder = builder;
    public override void Visit(Name name) => _builder.Append(name.Value);
    public override void Visit(NamedRuleList namedRuleList) => throw new NotImplementedException();
    public override void Visit(Rule.Alternative alternative) => throw new NotImplementedException();
    public override void Visit(Rule.Any any) => throw new NotImplementedException();
    public override void Visit(Rule.Concatenation concatenation) => throw new NotImplementedException();
    public override void Visit(Rule.Grouping grouping) => throw new NotImplementedException();
    public override void Visit(Rule.OptionalGrouping optionalGrouping) => throw new NotImplementedException();
    public override void Visit(Rule.Range range) => throw new NotImplementedException();
    public override void Visit(Rule.Reference reference) => throw new NotImplementedException();
    public override void Visit(Rule.SpecificRepetition specificRepetition) => throw new NotImplementedException();
    public override void Visit(Rule.Terminal terminal) => throw new NotImplementedException();
    public override void Visit(Rule.VariableRepetition variableRepetition) => throw new NotImplementedException();
    public override void Visit(RuleDefinition.New @new) => throw new NotImplementedException();
    public override void Visit(RuleDefinition.Incremental incremental) => throw new NotImplementedException();
    public override void Visit(RuleList ruleList) => throw new NotImplementedException();
}
