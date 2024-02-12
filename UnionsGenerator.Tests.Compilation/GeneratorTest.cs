namespace RhoMicro.CodeAnalysis.UnionsGenerator.Tests;

using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RhoMicro.CodeAnalysis.UnionsGenerator.Generators;
using System.Collections.Concurrent;

/// <summary>
/// Base class for tests verifying <see cref="UnionsGenerator"/> outputs.
/// </summary>
public abstract class GeneratorTest(IAssertStrategy assert)
{
    private readonly IAssertStrategy _assert = assert;

    //requiring >= C#11 due to file scoped modifiers
    private const LanguageVersion _targetLanguageVersion = LanguageVersion.CSharp11;
    private static readonly CSharpParseOptions _parseOptions =
        new(languageVersion: _targetLanguageVersion,
            documentationMode: DocumentationMode.Diagnose,
            kind: SourceCodeKind.Regular);
    /// <summary>
    /// Invokes an assertion on the union type implementation generated from a source.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="assertion"></param>
    /// <param name="unionTypeName"></param>
    public void UnionType(String source, Action<INamedTypeSymbol> assertion, String? unionTypeName = null)
    {
        _ = assertion ?? throw new ArgumentNullException(nameof(assertion));

        Compilation compilation = CreateCompilation(source, out var sourceTree);
        _ = RunGenerator(ref compilation);
        var declaration = sourceTree.GetRoot()
            .DescendantNodesAndSelf()
            .OfType<TypeDeclarationSyntax>()
            .SingleOrDefault(d => unionTypeName == null || d.Identifier.Text == unionTypeName);
        var symbol = compilation.GetSemanticModel(sourceTree)
            .GetDeclaredSymbol(declaration);
        _assert.NotNull(symbol);
        assertion.Invoke(symbol!);
    }
    /// <summary>
    /// Invokes an assertion on the result of running the generator once on a source.
    /// </summary>
    /// <param name="source"></param>
    /// <param name="assertion"></param>
    public void DriverResult(String source, Action<GeneratorDriverRunResult> assertion)
    {
        _ = assertion ?? throw new ArgumentNullException(nameof(assertion));

        Compilation compilation = CreateCompilation(source, out var _);
        var result = RunGenerator(ref compilation);
        assertion.Invoke(result);
    }

    private GeneratorDriverRunResult RunGenerator(ref Compilation compilation)
    {
        var generator = new UnionsGenerator();

        var driver = CSharpGeneratorDriver.Create(generator)
            .WithUpdatedParseOptions(_parseOptions);

        // Run the generation pass
        // (Note: the generator driver itself is immutable, and all calls return an updated version of the driver that you should use for subsequent calls)
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out compilation, out var diagnostics);

        // We can now assert things about the resulting compilation:
        _assert.True(diagnostics.IsEmpty); // there were no diagnostics created by the generators
        _assert.True(compilation.SyntaxTrees.Count() == 2 + UnionsGenerator.ConstantSourceTexts.Count);
        var aggregateDiagnostics = compilation.GetDiagnostics();
        _assert.Empty(aggregateDiagnostics); // verify the compilation with the added source has no diagnostics

        // Or we can look at the results directly:
        var result = driver.GetRunResult();

        // The runResult contains the combined results of all generators passed to the driver
        _assert.True(result.GeneratedTrees.Length == 1 + UnionsGenerator.ConstantSourceTexts.Count);
        _assert.True(result.Diagnostics.IsEmpty);

        return result;
    }

    private static CSharpCompilation CreateCompilation(String source, out SyntaxTree sourceTree)
    {
        var options = CreateCompilationOptions();
        sourceTree = CSharpSyntaxTree.ParseText(source, _parseOptions);

        var references =
            //TODO: constrain the set of available assemblies to encompass netstandard2
            //var loadedAssemblies = new Dictionary<String, String>();
            //foreach(var loadedAssembly in AppDomain.CurrentDomain.GetAssemblies())
            //{
            //    if(!loadedAssemblies.ContainsKey(loadedAssembly.FullName))
            //        loadedAssemblies.Add(loadedAssembly.FullName, loadedAssembly.Location);
            //}
            //new[]{
            //    typeof(Object),
            //    typeof(ConcurrentDictionary<,>)
            //}
            //.Select(b => b.Assembly.FullName)
            //.Where(loadedAssemblies.ContainsKey)
            //.Select(n => loadedAssemblies[n])
            AppDomain.CurrentDomain.GetAssemblies()
            .Select(a => a.Location)
            .Where(l => !String.IsNullOrEmpty(l))
            .Select(l => MetadataReference.CreateFromFile(l));

        var result = CSharpCompilation.Create(
            assemblyName: null,
            syntaxTrees: [sourceTree],
            references: references,
            options: options);

        return result;
    }
    private static CSharpCompilationOptions CreateCompilationOptions()
    {
        String[] args = ["/warnaserror"];
#pragma warning disable RS1035 // Do not use APIs banned for analyzers (not an analyzer????)
        var commandLineArguments = CSharpCommandLineParser.Default.Parse(args, baseDirectory: Environment.CurrentDirectory, sdkDirectory: Environment.CurrentDirectory);
#pragma warning restore RS1035 // Do not use APIs banned for analyzers
        var result = commandLineArguments.CompilationOptions
            .WithOutputKind(OutputKind.DynamicallyLinkedLibrary);

        return result;
    }
}
