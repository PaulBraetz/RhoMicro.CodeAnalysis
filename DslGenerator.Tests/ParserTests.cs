#pragma warning disable CA1819 // Properties should not return arrays
namespace RhoMicro.CodeAnalysis.DslGenerator.Tests;

using RhoMicro.CodeAnalysis.DslGenerator.Analysis;
using RhoMicro.CodeAnalysis.DslGenerator.Grammar;
using RhoMicro.CodeAnalysis.DslGenerator.Lexing;
using RhoMicro.CodeAnalysis.DslGenerator.Parsing;

using System;
using System.Diagnostics;
using System.Linq;

public class ParserTests
{
    public static Object[][] Data =>
        [
            [
                "rule=rule;",
                new RuleListBuilder().New("rule", b => b.Reference("rule")),
                Array.Empty<Int32>()
            ],
            [
                """rule="a"-"z";""",
                new RuleListBuilder().New("rule", b => b.Range('a', 'z')),
                Array.Empty<Int32>()
            ],
            [
                """rule= other / "a"-"z";""",
                new RuleListBuilder().New("rule", b => b.Alternative(b => b.Reference("other").Range('a', 'z'))),
                Array.Empty<Int32>()
            ],
            [
                "rule = rule; ",
                new RuleListBuilder().New("rule", b => b.Reference("rule")),
                Array.Empty<Int32>()
            ],
            [
                """

                 rule   =   # some commented stuff
                    rule; 
                """,
                new RuleListBuilder().New("rule", b => b.Reference("rule")),
                Array.Empty<Int32>()
            ],
            [
                """
                rule = [optionalRule];
                """,
                new RuleListBuilder().New("rule", b => b.OptionalGrouping(b => b.Reference("optionalRule"))),
                Array.Empty<Int32>()
            ],
            [
                """
                rule = (groupedRule);
                """,
                new RuleListBuilder().New("rule", b => b.Grouping(b => b.Reference("groupedRule"))),
                Array.Empty<Int32>()
            ],
            [
                """
                testRuleList;
                rule = (groupedRule);
                """,
                new RuleListBuilder("testRuleList").New("rule", b => b.Grouping(b => b.Reference("groupedRule"))),
                Array.Empty<Int32>()
            ],
            [
                "rule = (\"groupedTerminal\");",
                new RuleListBuilder().New("rule", b => b.Grouping(b => b.Terminal("groupedTerminal"))),
                Array.Empty<Int32>()
            ],
            [
                "rule = [\"groupedTerminal\"];",
                new RuleListBuilder().New("rule", b => b.OptionalGrouping(b => b.Terminal("groupedTerminal"))),
                Array.Empty<Int32>()
            ],
            [
            """
            testname = ["is this the only legal value?"] / # here is a comment
            *"no, it's not :)"; # and another comment
            """,
                new RuleListBuilder().New("testname", b =>
                    b.Alternative(b =>
                        b.OptionalGrouping(b =>
                            b.Terminal("is this the only legal value?"))
                        .VariableRepetition(b =>
                            b.Terminal("no, it's not :)"))
                    )),
                Array.Empty<Int32>()
            ],
            [
            """
            testname /= ["is this the only legal value?"] / # here is a comment
            *"no, it's not :)"; # and another comment
            """,
                new RuleListBuilder().Incremental("testname", b =>
                    b.Alternative(b =>
                        b.OptionalGrouping(b =>
                            b.Terminal("is this the only legal value?"))
                        .VariableRepetition(b =>
                            b.Terminal("no, it's not :)"))
                    )),
                Array.Empty<Int32>()
            ],
            [

                "rule=\"\";",
                new RuleListBuilder().New("rule", b =>
                    b.Terminal(String.Empty)
                ),
                Array.Empty<Int32>()
            ],
            [
                "Range = SingleAlpha \"-\" SingleAlpha;",
                new RuleListBuilder().New("Range", b =>
                    b.Reference("SingleAlpha").Terminal("-").Reference("SingleAlpha")),
                Array.Empty<Int32>()
            ],
            [
                """
                TestGrammarName;
                testname = ["is this the only legal value?"] / # here is a comment
                		   *"no, it's not :)"; # and another comment
                testname /= 77here (is *some more) "ruley" [goodness]; # :)
                more = defs;
                testname /= more;
                more /= no;
                """,
                new RuleListBuilder("TestGrammarName")
                    .New("testname", b =>
                        b.Alternative(b =>
                            b.OptionalGrouping(b => b.Terminal("is this the only legal value?"))
                            .VariableRepetition(b => b.Terminal("no, it's not :)"))))
                    .Incremental("testname", b =>
                        b.SpecificRepetition(77, b => b.Reference("here"))
                        .Grouping(b => b.Reference("is").VariableRepetition(b => b.Reference("some")).Reference("more"))
                        .Terminal("ruley")
                        .OptionalGrouping(b => b.Reference("goodness")))
                    .New("more", b => b.Reference("defs"))
                    .Incremental("testname", b => b.Reference("more"))
                    .Incremental("more", b => b.Reference("no")),
                Array.Empty<Int32>()
            ],
            [
                "TerminalValue = \"\\\"\" . \"\\\"\";",
                new RuleListBuilder().New("TerminalValue", b =>
                    b.Terminal("\\\"").Any().Terminal("\\\"")),
                Array.Empty<Int32>()
            ],
            [
                """
                RhoMicroBackusNaurForm;

                RuleList = [Name ";"] *RuleDefinition;
                RuleDefinition = Name "=" Rule ";";

                Rule = Binary / Unary / Primary;

                Binary = Unary / Range / Concatenation / Alternative;
                Concatenation = Rule Whitespace Rule;
                Alternative = Rule "/" Rule;
                Range = SingleAlpha "-" SingleAlpha;

                Unary =  Primary / VariableRepetition / SpecificRepetition;
                VariableRepetition = "*" Rule;
                SpecificRepetition = Digit Rule;

                Primary = Grouping / OptionalGrouping / TerminalOrNameOrAny;
                Grouping = "(" Rule ")";
                OptionalGrouping = "[" Rule "]";
                TerminalOrNameOrAny = Terminal / Name / Any ;
                Terminal = "\"" . "\"";
                Name = Alpha;
                Any = ".";

                Trivia = Whitespace / NewLine;
                Whitespace = Space / Tab *Whitespace;
                Space = " ";
                Tab = "	";
                NewLine = "\n" / "\r\n" / "\r";
                SingleAlpha = "a"-"z" / "A"-"Z" / "_";
                Alpha = SingleAlpha *SingleAlpha;
                Digit = "0"-"9" *Digit;
                
                """,
                new RuleListBuilder("RhoMicroBackusNaurForm")

                .New("RuleList", b => b.OptionalGrouping(b => b.Reference("Name").Terminal(";")).VariableRepetition("RuleDefinition"))
                .New("RuleDefinition", b => b.Reference("Name").Terminal("=").Reference("Rule").Terminal(";"))

                .New("Rule", b => b.Alternative("Binary", "Unary", "Primary"))

                .New("Binary", b => b.Alternative("Unary", "Range", "Concatenation", "Alternative"))
                .New("Concatenation", b => b.Concatenation("Rule", "Whitespace", "Rule"))
                .New("Alternative", b => b.Reference("Rule").Terminal("/").Reference("Rule"))
                .New("Range", b => b.Reference("SingleAlpha").Terminal("-").Reference("SingleAlpha"))

                .New("Unary", b => b.Alternative("Primary", "VariableRepetition", "SpecificRepetition"))
                .New("VariableRepetition", b => b.Terminal("*").Reference("Rule"))
                .New("SpecificRepetition", b => b.Concatenation("Digit", "Rule"))

                .New("Primary", b => b.Alternative("Grouping", "OptionalGrouping", "TerminalOrNameOrAny"))
                .New("Grouping", b => b.Terminal("(").Reference("Rule").Terminal(")"))
                .New("OptionalGrouping", b => b.Terminal("[").Reference("Rule").Terminal("]"))
                .New("TerminalOrNameOrAny", b => b.Alternative("Terminal", "Name", "Any"))
                .New("Terminal", b => b.Terminal("\\\"").Any().Terminal("\\\""))
                .New("Name", b => b.Reference("Alpha"))
                .New("Any", b => b.Terminal("."))

                .New("Trivia", b => b.Alternative("Whitespace", "NewLine"))
                .New("Whitespace", b => b.Alternative("Space", "Tab").VariableRepetition("Whitespace"))
                .New("Space", b => b.Terminal(" "))
                .New("Tab", b => b.Terminal("\t"))
                .New("NewLine", b => b.Alternative(b => b.Terminal("\\n").Terminal("\\r\\n").Terminal("\\r")))
                .New("SingleAlpha", b => b.Alternative(b => b.Range('a', 'z').Range('A', 'Z').Terminal("_")))
                .New("Alpha", b => b.Reference("SingleAlpha").VariableRepetition("SingleAlpha"))
                .New("Digit", b => b.Range('0', '9').VariableRepetition("Digit")),

                Array.Empty<Int32>()
            ]
        ];

    [Theory]
    [MemberData(nameof(Data))]
    public void ParsesCorrectly(String source, Object expectedWeak, Int32[] expectedDiagnosticIds)
    {
        //Arrange
#pragma warning disable CA1062 // Validate arguments of public methods
        var expectedRuleList = ((RuleListBuilder)expectedWeak).Build();
#pragma warning restore CA1062 // Validate arguments of public methods
        var tokenizeResult = new Tokenizer().Tokenize(source, default, String.Empty);
        var parser = new Parser();

        //Act
        var (actualRuleList, actualDiagnostics) = parser.Parse(tokenizeResult, default);

        //Assert
        try
        {
            Assert.Equal(expectedRuleList, actualRuleList);
            VerifyDiagnostics(expectedDiagnosticIds, actualDiagnostics);
        } catch
        {
            var expected = expectedRuleList.ToDisplayString(default).Split('\n');
            var actual = actualRuleList.ToDisplayString(default).Split('\n');
            var comparisonStringParts = expected
                .Select((e, i) => (e, i))
                .Where((t) => actual.ElementAtOrDefault(t.i) != t.e)
                .Select((t, i) => $"{t.e} # EXPECTED\n{actual.ElementAtOrDefault(t.i) ?? "# MISSING"} # ACTUAL");
            var comparisonString = String.Join("\n\n", comparisonStringParts);

            Debugger.Break();
            _ = parser.Parse(tokenizeResult, default);
            throw;
        }
    }
    private static void VerifyDiagnostics(Int32[] expectedDiagnosticIds, DiagnosticsCollection actualDiagnostics)
    {
        var actualDiagnosticIdCounts = actualDiagnostics.GroupBy(d => d.Descriptor.Id)
                    .Select(g => (id: g.Key, count: g.Count()))
                    .ToDictionary(t => t.id, t => t.count);
        foreach(var (id, expectedCount) in expectedDiagnosticIds.GroupBy(id => id).Select(g => (id: g.Key, count: g.Count())))
        {
            var actualCount = actualDiagnosticIdCounts.TryGetValue(id, out var c) ? c : -1;
            Assert.Equal(expectedCount, actualCount);
        }
    }
}
