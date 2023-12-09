namespace RhoMicro.CodeAnalysis.Common.Tests;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

/// <summary>
/// Represents a fixture for testing expansion providers.
/// </summary>
/// <typeparam name="TMacro">The type of placeholder to provide expansions for.</typeparam>
/// <param name="Provider">The provider under test.</param>
/// <param name="Expected">The expected result of the provider.</param>
internal readonly record struct ExpansionProviderFixture<TMacro>(
    IMacroExpansion<TMacro> Provider,
    String Expected)
{
    /// <summary>
    /// Asserts the result of the provider to be equal to <see cref="Expected"/>.
    /// Note that the output produced by <see cref="Provider"/> will be formatted 
    /// using default options. (<see cref="ExpandingMacroStringBuilder.Format{TMacro}(IExpandingMacroStringBuilder{TMacro}, CSharpBuilderFormattingOptions?)"/>)
    /// </summary>
    public void Assert()
    {
        var actual = ExpandingMacroStringBuilder.Create<TMacro>()
            .AppendMacro(Provider.Macro)
            .Receive(Provider)
            .Format()
            .Build();
        var actualTree = CSharpSyntaxTree.ParseText(actual);
        var expectedTree = CSharpSyntaxTree.ParseText(Expected);

        Xunit.Assert.True(actualTree.IsEquivalentTo(expectedTree), "Actual syntax tree is not equivalent to expected syntax tree.");
    }
}

/// <summary>
/// Provides factory methods for expansion provider fixtures.
/// </summary>
internal static class ExpansionProviderFixture
{
    public static ExpansionProviderFixture<TMacro> Create<TMacro>(String source, String expected, Func<SyntaxTree, SemanticModel, IMacroExpansion<TMacro>> expansionFactory)
    {
        var tree = CSharpSyntaxTree.ParseText(source);
        var compilation = CSharpCompilation.Create("TestAssembly", new[] { tree });
        var model = compilation.GetSemanticModel(tree);
        var expansion = expansionFactory.Invoke(tree, model);
        var result = new ExpansionProviderFixture<TMacro>(expansion, expected);

        return result;
    }
    /// <summary>
    /// Creates a new fixture for testing a expansion provider.
    /// </summary>
    /// <typeparam name="TMacro">The type of placeholder to provide expansions for.</typeparam>
    /// <param name="expected">The expected, formatted output of the expansion provider, given the model created.</param>
    /// <param name="provider">The provider to test.</param>
    /// <returns>A new fixture for the provider provided.</returns>
    public static ExpansionProviderFixture<TMacro> Create<TMacro>(String expected, IMacroExpansion<TMacro> provider)
    {
        var result = new ExpansionProviderFixture<TMacro>(provider, expected);

        return result;
    }
    /// <summary>
    /// Creates a new fixture for testing a expansion provider.
    /// </summary>
    /// <typeparam name="TMacro">The type of placeholder to provide expansions for.</typeparam>
    /// <param name="expected">The expected, formatted output of the expansion provider, given the model created.</param>
    /// <param name="macro">The macro to test.</param>
    /// <param name="strategy">The macro expansion strategy to test.</param>
    /// <returns>A new fixture for the provider provided.</returns>
    public static ExpansionProviderFixture<TMacro> Create<TMacro>(String expected, TMacro macro, Action<IExpandingMacroStringBuilder<TMacro>, CancellationToken> strategy)
    {
        var result = new ExpansionProviderFixture<TMacro>(MacroExpansion.Create(macro, strategy), expected);

        return result;
    }
    /// <summary>
    /// Creates a new fixture for testing a expansion provider.
    /// </summary>
    /// <typeparam name="TMacro">The type of placeholder to provide expansions for.</typeparam>
    /// <typeparam name="TProvider">The type of provider to test.</typeparam>
    /// <param name="expected">The expected, formatted output of the expansion provider, given the model created.</param>
    /// <returns>A new fixture for the provider provided.</returns>
    public static ExpansionProviderFixture<TMacro> Create<TProvider, TMacro>(
        String expected)
        where TProvider : IMacroExpansion<TMacro>, new() =>
        Create(expected, new TProvider());
}
