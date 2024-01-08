namespace RhoMicro.CodeAnalysis.DslGenerator.Grammar;

using System.Collections.Immutable;

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
sealed partial class Rule;

[UnionType(typeof(String), Alias = "Value")]
readonly partial struct RuleName;

[UnionType(typeof(ImmutableArray<RuleDefinition>), Alias = "Rules")]
readonly partial struct RuleList;

readonly record struct RuleDefinition(RuleName Name, Rule Rule);

readonly record struct OptionalSequence(ImmutableArray<Rule> Rules);
readonly record struct SpecificRepetition(Rule Rule, Int32 Count);
readonly record struct VariableRepetition(ImmutableArray<Rule> Rules);
readonly record struct SequenceGroup(ImmutableArray<Rule> Rules);
readonly record struct IncrementalAlternative(Rule Left, Rule Right);
readonly record struct Alternative(Rule Left, Rule Right);
readonly record struct Concatenation(Rule Left, Rule Right);
readonly record struct Terminal(String Value);
