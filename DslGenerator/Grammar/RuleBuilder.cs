#if DSL_GENERATOR
namespace RhoMicro.CodeAnalysis.DslGenerator.Grammar;
#else
#pragma warning disable
#nullable enable
namespace RhoMicro.CodeAnalysis.DslGenerator.Generated.Grammar;
#endif

using System;
using System.Collections.Generic;

#if DSL_GENERATOR
[IncludeFile]
#endif
readonly struct RuleBuilder
{
    public RuleBuilder() { }
    private readonly List<Rule> _rules = [];
    public RuleBuilder Reference(String value)
    {
        _rules.Add(new Rule.Reference(new(value)));
        return this;
    }
    public RuleBuilder Terminal(String value)
    {
        _rules.Add(new Rule.Terminal(value));
        return this;
    }
    public RuleBuilder Alternative(Action<RuleBuilder> buildRule)
    {
        var builder = new RuleBuilder();
        buildRule.Invoke(builder);
        if(builder._rules.Count == 0)
        {
            return this;
        }

        var rule =
            builder._rules.Count > 1 ?
            builder._rules.Aggregate((left, right) => new Rule.Alternative(left, right)) :
            builder.Build();
        _rules.Add(rule);

        return this;
    }
    public RuleBuilder VariableRepetition(Action<RuleBuilder> buildRule)
    {
        var rule = GetRule(buildRule);
        var repeatedRule = new Rule.VariableRepetition(rule);
        _rules.Add(repeatedRule);

        return this;
    }
    public RuleBuilder SpecificRepetition(Int32 count, Action<RuleBuilder> buildRule)
    {
        var rule = GetRule(buildRule);
        var repeatedRule = new Rule.SpecificRepetition(count, rule);
        _rules.Add(repeatedRule);

        return this;
    }
    public RuleBuilder Grouping(Action<RuleBuilder> buildRule)
    {
        var rule = GetRule(buildRule);
        var repeatedRule = new Rule.Grouping(rule);
        _rules.Add(repeatedRule);

        return this;
    }
    public RuleBuilder OptionalGrouping(Action<RuleBuilder> buildRule)
    {
        var rule = GetRule(buildRule);
        var repeatedRule = new Rule.OptionalGrouping(rule);
        _rules.Add(repeatedRule);

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
        _rules.Aggregate((left, right) => new Rule.Concatenation(left, right)) :
        throw new InvalidOperationException("Unable to build rule as no builder instruction has been called.");
}
