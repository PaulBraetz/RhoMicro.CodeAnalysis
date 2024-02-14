namespace RhoMicro.CodeAnalysis.DslGenerator.Grammar;

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