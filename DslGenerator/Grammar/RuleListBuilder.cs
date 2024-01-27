namespace RhoMicro.CodeAnalysis.DslGenerator.Grammar;

using System;

#if DSL_GENERATOR
[IncludeFile]
#endif
readonly struct RuleListBuilder
{
    public RuleListBuilder(String nameValue) =>
        _name = new(nameValue);
    public RuleListBuilder() =>
        _name = null;

    private readonly List<RuleDefinition> _definitions = [];
    private readonly Name? _name;

    public override String ToString() => Build().ToString();
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
