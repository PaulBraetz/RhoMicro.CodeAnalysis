namespace RhoMicro.CodeAnalysis.DslGenerator.Grammar;

using System.Collections.Immutable;
using System.Linq;

#if DSL_GENERATOR
[IncludeFile]
#endif
[UnionType(typeof(Terminal))]
[UnionType(typeof(Concatenation))]
[UnionType(typeof(Alternative))]
[UnionType(typeof(SequenceGroup))]
[UnionType(typeof(VariableRepetition))]
[UnionType(typeof(SpecificRepetition))]
[UnionType(typeof(OptionalSequence))]
sealed partial class Rule
{
    public readonly record struct Alternative(Rule Left, Rule Right);
    public readonly record struct Concatenation(Rule Left, Rule Right);
    public readonly record struct SpecificRepetition(Rule Rule, Int32 Count);
    public readonly record struct VariableRepetition(Rule Rule);
    public readonly record struct SequenceGroup(Rule Left, Rule Right);
    public readonly record struct OptionalSequence(Rule Left, Rule Right);
    public readonly record struct Terminal(String Value);
}

[UnionType(typeof(String), Alias = "Value")]
readonly partial struct RuleName;

[UnionType(typeof(ImmutableArray<RuleDefinition>), Alias = "Rules")]
readonly partial struct RuleList
{
    public Boolean Equals(RuleList other) => other.AsRules.SequenceEqual(AsRules);
    public override Int32 GetHashCode() => AsRules.Aggregate(997021164, (hc, def) => hc * -1521134295 + def.GetHashCode());
}

[UnionType(typeof(New))]
[UnionType(typeof(Incremental))]
readonly partial struct RuleDefinition
{
    public readonly record struct New(RuleName Name, Rule Rule);
    public readonly record struct Incremental(RuleName Name, Rule Rule);
}
