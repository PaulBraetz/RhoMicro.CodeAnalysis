namespace RhoMicro.CodeAnalysis.DslGenerator.Grammar;

using System.Linq;

#if DSL_GENERATOR
[IncludeFile]
#endif
static class ListExtensions
{
    public static IReadOnlyList<RuleDefinition> Unify(this IReadOnlyList<RuleDefinition> definitions) =>
        definitions.Aggregate(new Dictionary<Name, Rule>(), (defs, def) =>
        {
            var (name, rule) = def;
            //previous definition?
            if(!defs.TryGetValue(name, out var previousRule))
            {
                //new definition
                defs[name] = rule;
                return defs;
            }

            //redefinition?
            if(def is RuleDefinition.New)
            {
                //TODO: make illegal?
                defs[name] = rule;
                return defs;
            }

            //incremental?
            if(def is RuleDefinition.Incremental)
            {
                //make sure left is grouped to maintain semantics
                // rule /= a / b / c
                // rule = (rule) or a or b or c
                var groupedPreviousRule = previousRule is Rule.Grouping or Rule.OptionalGrouping ?
                    previousRule :
                    new Rule.Grouping(previousRule);

                var alternative = new Rule.Alternative(groupedPreviousRule, rule);
                defs[name] = alternative;
                return defs;
            }

            throw new InvalidOperationException($"Encountered unknown rule definition type: {def.GetType()}");
        }).Select(kvp => new RuleDefinition.New(kvp.Key, kvp.Value)).ToList();
}