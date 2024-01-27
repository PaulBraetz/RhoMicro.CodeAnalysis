namespace RhoMicro.CodeAnalysis.DslGenerator.Grammar;
using System;

sealed class DisplayStringVisitor : SyntaxNodeVisitor
{
    public DisplayStringVisitor(IndentedStringBuilder builder) => _builder = builder;
    public DisplayStringVisitor() : this(new()) { }
    private readonly IndentedStringBuilder _builder;
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
abstract class SyntaxNodeVisitor
{
    public abstract void Visit(Name name);
    public abstract void Visit(NamedRuleList namedRuleList);
    public abstract void Visit(Rule.Alternative alternative);
    public abstract void Visit(Rule.Any any);
    public abstract void Visit(Rule.Concatenation concatenation);
    public abstract void Visit(Rule.Grouping grouping);
    public abstract void Visit(Rule.OptionalGrouping optionalGrouping);
    public abstract void Visit(Rule.Range range);
    public abstract void Visit(Rule.Reference reference);
    public abstract void Visit(Rule.SpecificRepetition specificRepetition);
    public abstract void Visit(Rule.Terminal terminal);
    public abstract void Visit(Rule.VariableRepetition variableRepetition);
    public abstract void Visit(RuleDefinition.New @new);
    public abstract void Visit(RuleDefinition.Incremental incremental);
    public abstract void Visit(RuleList ruleList);
}