namespace RhoMicro.CodeAnalysis.DslGenerator.Grammar;

using System;
using System.Collections.Generic;

using static Rule;

#if DSL_GENERATOR
[IncludeFile]
#endif
readonly struct RuleBuilder
{
    public RuleBuilder()
    : this(isAlternative: false)
    { }
    private RuleBuilder(Boolean isAlternative) => _isAlternative = isAlternative;
    private readonly List<Rule> _rules = [];
    private readonly Boolean _isAlternative;

    public override String ToString() => _rules.Count == 0 ? GetType().ToString() : Build().ToString();
    public RuleBuilder Reference(String value)
    {
        _rules.Add(new Reference(new(value)));
        return this;
    }
    public RuleBuilder Terminal(String value)
    {
        _rules.Add(new Terminal(value));
        return this;
    }
    public RuleBuilder Any()
    {
        _rules.Add(Rule.Any.Instance);
        return this;
    }
    public RuleBuilder Range(Char start, Char end)
    {
        _rules.Add(new Rule.Range(new(start.ToString()), new(end.ToString())));
        return this;
    }
    public RuleBuilder Concatenation(params String[] referenceNames) =>
        Concatenation(b => _ = referenceNames.Aggregate(b, (b, n) => b.Reference(n)));
    public RuleBuilder Concatenation(Action<RuleBuilder> buildRule) => AppendBinary(buildAlternative: false, buildRule);
    public RuleBuilder Alternative(params String[] referenceNames) =>
        Alternative(b => _ = referenceNames.Aggregate(b, (b, n) => b.Reference(n)));
    public RuleBuilder Alternative(Action<RuleBuilder> buildRule) => AppendBinary(buildAlternative: true, buildRule);
    private RuleBuilder AppendBinary(Boolean buildAlternative, Action<RuleBuilder> buildRule)
    {
        if(buildAlternative == _isAlternative)
        {
            buildRule.Invoke(this);
            return this;
        }

        var builder = new RuleBuilder(buildAlternative);
        buildRule.Invoke(builder);
        if(builder._rules.Count == 0)
        {
            return this;
        }

        var rule =
            builder._rules.Count > 1 ?
            builder._rules.Aggregate((left, right) => new Alternative(left, right)) :
            builder.Build();
        _rules.Add(rule);

        return this;
    }
    public RuleBuilder VariableRepetition(String referenceName)
    {
        var rule = new Reference(new(referenceName));
        var repeatedRule = new VariableRepetition(rule);
        _rules.Add(repeatedRule);

        return this;
    }
    public RuleBuilder VariableRepetition(Action<RuleBuilder> buildRule)
    {
        var rule = GetRule(buildRule);
        var repeatedRule = new VariableRepetition(rule);
        _rules.Add(repeatedRule);

        return this;
    }
    public RuleBuilder SpecificRepetition(Int32 count, String referenceName)
    {
        var rule = new Reference(new(referenceName));
        var repeatedRule = new SpecificRepetition(count, rule);
        _rules.Add(repeatedRule);

        return this;
    }
    public RuleBuilder SpecificRepetition(Int32 count, Action<RuleBuilder> buildRule)
    {
        var rule = GetRule(buildRule);
        var repetition = new SpecificRepetition(count, rule);
        _rules.Add(repetition);

        return this;
    }
    public RuleBuilder Grouping(Action<RuleBuilder> buildRule)
    {
        var rule = GetRule(buildRule);
        var grouping = new Grouping(rule);
        _rules.Add(grouping);

        return this;
    }
    public RuleBuilder OptionalGrouping(String referenceName)
    {
        var rule = new Reference(new(referenceName));
        var grouping = new OptionalGrouping(rule);
        _rules.Add(grouping);

        return this;
    }
    public RuleBuilder OptionalGrouping(Action<RuleBuilder> buildRule)
    {
        var rule = GetRule(buildRule);
        var grouping = new OptionalGrouping(rule);
        _rules.Add(grouping);

        return this;
    }

    private static Rule GetRule(Action<RuleBuilder> buildRule)
    {
        var builder = new RuleBuilder();
        buildRule.Invoke(builder);
        var rule = builder.Build();
        return rule;
    }

    public Rule Build() => _rules.Count == 1 ?
        _rules[0] :
        _rules.Count > 1 ?
        _rules.Aggregate((left, right) => new Concatenation(left, right)) :
        throw new InvalidOperationException("Unable to build rule as no builder instruction has been called.");
}
