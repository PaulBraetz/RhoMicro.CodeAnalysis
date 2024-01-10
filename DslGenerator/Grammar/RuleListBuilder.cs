#if DSL_GENERATOR
namespace RhoMicro.CodeAnalysis.DslGenerator.Grammar;
#else
#pragma warning disable
#nullable enable
namespace RhoMicro.CodeAnalysis.DslGenerator.Generated.Grammar;
#endif

using System;

#if DSL_GENERATOR
[IncludeFile]
#endif
sealed class RuleListBuilder(String? nameValue = null)
{
    private readonly List<RuleDefinition> _definitions = [];
    private readonly Name? _name = nameValue != null ? new(nameValue) : null;

    public RuleListBuilder New(String nameValue, Action<RuleBuilder> buildRule)
    {
        var rule = GetRule(buildRule);
        var ruleDefinition = new RuleDefinition.New(new(nameValue), rule);
        _definitions.Add(ruleDefinition);

        return this;
    }
    public RuleListBuilder Incremental(String nameValue, Action<RuleBuilder> buildRule)
    {
        var rule = GetRule(buildRule);
        var ruleDefinition = new RuleDefinition.Incremental(new(nameValue), rule);
        _definitions.Add(ruleDefinition);

        return this;
    }
    private static Rule GetRule(Action<RuleBuilder> buildRule)
    {
        var ruleBuilder = new RuleBuilder();
        buildRule.Invoke(ruleBuilder);
        var rule = ruleBuilder.Build();

        return rule;
    }
    public RuleList Build() =>
        _name != null ?
        new NamedRuleList(_name, _definitions) :
        new RuleList(_definitions);
}
