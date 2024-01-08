namespace RhoMicro.CodeAnalysis.DslGenerator.TestApp;

using RhoMicro.CodeAnalysis.DslGenerator.Grammar;

static class TestGrammar
{
    static void Foo()
    {
        var rule = new Terminal("The only legal value.");
        _ = new RuleDefinition("SingleRule", rule);
    }
}
