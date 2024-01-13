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

    }

    [RhoMicro.CodeAnalysis.GeneratedRuleList("rule=name;")]
    private static partial RuleList AttributeGenerated();
}