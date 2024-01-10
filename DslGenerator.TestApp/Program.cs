using RhoMicro.CodeAnalysis.DslGenerator.Generated.Grammar;
using RhoMicro.CodeAnalysis.DslGenerator.Generated.Lexing;
using RhoMicro.CodeAnalysis.DslGenerator.Generated.Parsing;

var ruleList = new RuleListBuilder()
    .New("firstRule", b => b.Reference("onlyLegalRule"))
    .Incremental("firstRule", b => b.Terminal("or is it?"))
    .Build();

Console.WriteLine(ruleList);

var generatedList = RuleLists.TestGrammarName;
Console.WriteLine(generatedList);

var (parsedList, _) = new Parser().Parse(new Tokenizer().Tokenize("rule_one = never trust \"the generator\";", default), default);
Console.WriteLine(parsedList);
