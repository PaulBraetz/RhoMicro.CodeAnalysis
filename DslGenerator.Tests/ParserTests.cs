namespace RhoMicro.CodeAnalysis.DslGenerator.Tests;

using RhoMicro.CodeAnalysis.DslGenerator.Analysis;
using RhoMicro.CodeAnalysis.DslGenerator.Grammar;
using RhoMicro.CodeAnalysis.DslGenerator.Lexing;
using RhoMicro.CodeAnalysis.DslGenerator.Parsing;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xunit.Sdk;

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
            Debugger.Break();
            _ = expectedRuleList.Equals(actualRuleList);
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
