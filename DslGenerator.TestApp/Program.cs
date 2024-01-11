namespace DslGenerator.TestApp;

using RhoMicro.CodeAnalysis.DslGenerator.Grammar;
using RhoMicro.CodeAnalysis.DslGenerator.Grammar.Generated;
using RhoMicro.CodeAnalysis.DslGenerator.Lexing;
using RhoMicro.CodeAnalysis.DslGenerator.Parsing;

using System.Text.RegularExpressions;
internal partial class Program
{
    private static void Main(String[] args)
    {
        var unifiedList = GeneratedRuleLists.RhoMicroBackusNaurForm with { Definitions = GeneratedRuleLists.RhoMicroBackusNaurForm.Definitions.Unify() };
        Console.WriteLine(unifiedList);

        var ruleList = new RuleListBuilder()
            .New("firstRule", b => b.Reference("onlyLegalRule"))
            .Incremental("firstRule", b => b.Terminal("or is it?"))
            .Build();

        Console.WriteLine(ruleList);

        var generatedList = GeneratedRuleLists.TestGrammarName;
        Console.WriteLine(generatedList);
        var unifiedGeneratedList = generatedList with { Definitions = generatedList.Definitions.Unify() };
        Console.WriteLine(unifiedGeneratedList);

        var (parsedList, _) = Parser.Instance.Parse("rule_one = never trust \"the generator\";", default);
        Console.WriteLine(parsedList);

        Console.WriteLine(AttributeGenerated().ToDisplayString());

        Console.WriteLine(new RuleListBuilder("RangeRule").New("rule", b => b.Range('a', 'z')));
    }

    [RhoMicro.CodeAnalysis.GeneratedRuleList("rule=name;")]
    private static partial RuleList AttributeGenerated();
}