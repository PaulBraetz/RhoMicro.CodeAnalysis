namespace DslGenerator.TestApp;

using RhoMicro.CodeAnalysis.DslGenerator.Generated.Grammar;
using RhoMicro.CodeAnalysis.DslGenerator.Generated.Lexing;
using RhoMicro.CodeAnalysis.DslGenerator.Generated.Parsing;

using System.Text.RegularExpressions;
internal partial class Program
{
    private static void Main(string[] args)
    {
        Console.WriteLine(FileGeneratedRuleLists.RhoMicroBackusNaurForm);
        var ruleList = new RuleListBuilder()
            .New("firstRule", b => b.Reference("onlyLegalRule"))
            .Incremental("firstRule", b => b.Terminal("or is it?"))
            .Build();

        Console.WriteLine(ruleList);

        var generatedList = FileGeneratedRuleLists.TestGrammarName;
        Console.WriteLine(generatedList);

        var (parsedList, _) = new Parser().Parse(new Tokenizer().Tokenize("rule_one = never trust \"the generator\";", default), default);
        Console.WriteLine(parsedList);

    }

    [RhoMicro.CodeAnalysis.GeneratedRuleList("rule=name;")]
    private static partial RuleList AttributeGenerated();

    [GeneratedRegex("abc")]
    private static partial Regex Get();
}