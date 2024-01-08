namespace RhoMicro.CodeAnalysis.UnionsGenerator.Generators;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using RhoMicro.CodeAnalysis;
using RhoMicro.CodeAnalysis.UtilityGenerators.Library;
using RhoMicro.CodeAnalysis.UnionsGenerator.Analyzers;
using RhoMicro.CodeAnalysis.UnionsGenerator.Models;

using System;
using System.Text;
using System.Threading;
using System.Diagnostics;
using RhoMicro.CodeAnalysis.UnionsGenerator.Generators.Expansions;

enum Macro
{
    Head,
    NestedClasses,
    Constructors,
    Fields,
    Factories,
    Switch,
    Match,
    RepresentedTypes,
    IsAsProperties,
    IsAsFunctions,
    GetHashcode,
    Equals,
    ToString,
    Conversion,
    Tail,
    ConversionFunctionsCache
}
[Generator(LanguageNames.CSharp)]
internal class UnionsGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        //TODO: implement model for efficient equality
        //TODO: implement incremental model building?
        var provider = context.SyntaxProvider.ForUnionTypeAttribute(
            static (n, t) => Util.IsGeneratorTarget(n),
            static (c, t) =>
            {
                var model = TargetDataModel.Create((TypeDeclarationSyntax)c.TargetNode, c.SemanticModel);

                var context = GeneratorContext.Create<Macro, TargetDataModel>(
                    model,
                    configureSourceTextBuilder: b => b.AppendHeader("RhoMicro.CodeAnalysis.UnionsGenerator")
                        .AppendMacros(t)
                        .Format()
                        .WithOperators<Macro, TargetDataModel>(t)
                        .Receive(new Head(model), t)
                        .Receive(new NestedTypes(model), t)
                        .Receive(new Constructors(model), t)
                        .Receive(new Fields(model), t)
                        .Receive(new Factories(model), t)
                        .Receive(new Tail(model), t)
                        .Receive(new RepresentedTypes(model), t)
                        .Receive(new IsAsProperties(model), t)
                        .Receive(new IsAsFunctions(model), t)
                        .Receive(new Match(model), t)
                        .Receive(new GetHashCode(model), t)
                        .Receive(new Equals(model), t)
                        .Receive(new ConversionFunctionsCache(model), t)
                        .Receive(new Expansions.Switch(model), t)
                        .Receive(ToStringFunction.Create(model), t)
                        .Receive(new Expansions.Conversion(model), t),
                    configureDiagnosticsAccumulator: d => d
                        .DiagnoseNonHiddenSeverities()
                        .ReportOnlyNoneLocations()
                        .Receive(Providers.All, t));

                var source = BuildSource(context, t);

                var hintName = model.Symbol.ToHintName();
                var hintNameWithExtension = $"{hintName}.g.cs";
                var result = (source, hintName: hintNameWithExtension);

                if(TryGetSourceEmission(model, (source, hintName), out var emissionResult, t))
                {
                    return (result, emissionResult);
                }

                return (result, null);
            });

        context.RegisterSourceOutput(provider, static (c, t) =>
        {
            t.result.source.AddToContext(c, t.result.hintName);
            if(t.emissionResult.HasValue)
            {
                c.AddSource(t.emissionResult.Value.hintName, t.emissionResult.Value.source);
            }
        });
        context.RegisterPostInitializationOutput(static c => c.AddSource("Util.g.cs", ConstantSources.Util));
        context.RegisterPostInitializationOutput(static c => c.AddSource($"{nameof(UnionFactoryAttribute)}.g.cs", UnionFactoryAttribute.SourceText));
        context.RegisterPostInitializationOutput(static c => c.AddSource($"{nameof(UnionTypeAttribute)}.g.cs", UnionTypeAttribute.SourceText));
        context.RegisterPostInitializationOutput(static c => c.AddSource($"{nameof(RelationAttribute)}.g.cs", RelationAttribute.SourceText));
        context.RegisterPostInitializationOutput(static c => c.AddSource($"{nameof(UnionTypeSettingsAttribute)}.g.cs", UnionTypeSettingsAttribute.SourceText));
    }
    private static Boolean TryGetSourceEmission(TargetDataModel model, (GeneratorContextBuildResult<TargetDataModel> source, String hintName) s, out (String source, String hintName)? result, CancellationToken cancellationToken)
    {
        result = default;
        if(!model.Annotations.Settings.EmitGeneratedSourceCode)
            return false;

        var builder = ExpandingMacroStringBuilder.Create<Macro>().WithOperators(cancellationToken);
        Head.CreateEmissionHead(model).Expand(builder, cancellationToken);
        _ = builder *
            "public const String GeneratedSourceCode =" /
            "\"\"\"" /
            s.source.SourceText /
            "\"\"\";";
        new Tail(model).Expand(builder, cancellationToken);

        var resultSource = builder.Build();
        var resultHintName = $"{s.hintName}_Source.g.cs";

        result = (resultSource, resultHintName);
        return true;
    }
    private static GeneratorContextBuildResult<TargetDataModel> BuildSource(IGeneratorContext<Macro, TargetDataModel> context, CancellationToken t)
    {
        var source = context.BuildSource(t);

        var syntaxTree = CSharpSyntaxTree.ParseText(
            source.SourceText,
            new CSharpParseOptions(LanguageVersion.CSharp8, DocumentationMode.Diagnose, SourceCodeKind.Regular),
            cancellationToken: t);
        var diagnostics = syntaxTree.GetDiagnostics(t);
        if(diagnostics.Any())
        {
            var newSourceText = $"/*\nThe generator failed to generate diagnostics-free source code.\nThis indicates a bug in the generator.\nPlease report this issue to the maintainer at https://github.com/PaulBraetz/RhoMicro.CodeAnalysis/issues.\n{String.Join("\n", diagnostics)}\n*/\n{String.Concat(Enumerable.Repeat('/', 150))}\n{source.SourceText}";
            source = source.WithSourceText(newSourceText);
        }

        return source;
    }
}
