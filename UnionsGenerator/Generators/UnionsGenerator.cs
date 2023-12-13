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
    Tail
}
[Generator(LanguageNames.CSharp)]
internal class UnionsGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        //TODO: implement model for efficient equality
        //TODO: implement incremental model building?
        var providers = context.SyntaxProvider.ForUnionTypeAttribute(
            static (n, t) => n is TypeDeclarationSyntax tds && tds.Modifiers.Any(SyntaxKind.PartialKeyword) && n is not RecordDeclarationSyntax,
            static (c, t) =>
            {
                var target = c.TargetNode as TypeDeclarationSyntax ?? throw new Exception("Target node was not type declaration.");
                var model = TargetDataModel.Create(target, c.SemanticModel);

                var context = GeneratorContext.Create<Macro, TargetDataModel>(
                    model,
                    configureSourceTextBuilder: b =>b.AppendHeader("RhoMicro.CodeAnalysis.UnionsGenerator")
                        .AppendMacros(t)
                        .Format()
                        .GetOperators<Macro, TargetDataModel>(t) +
                        new Head(model) +
                        new NestedTypes(model) +
                        new Constructors(model) +
                        new Fields(model) +
                        new Factories(model) +
                        new Tail(model) +
                        new RepresentedTypes(model) +
                        new IsAsProperties(model) +
                        new IsAsFunctions(model) +
                        new Match(model) +
                        new Expansions.Switch(model) +
                        new GetHashCode(model) +
                        new Equals(model) +
                        ToStringFunction.Create(model) +
                        Expansions.Conversion.Create(model),
                    configureDiagnosticsAccumulator: d => d
                        .ReportNonHiddenSeverities()
                        .DiagnoseNonHiddenSeverities()
                        .Receive(Providers.All, t));

                var source = BuildSource(context, t);

                var hintName = $"{model.Symbol.ToHintName()}.g.cs";

                return (source, hintName);
            });

        context.RegisterSourceOutput(providers, static (c, t) => t.source.AddToContext(c, t.hintName));
        context.RegisterPostInitializationOutput(static c => c.AddSource("Util.g.cs", ConstantSources.Util));
        context.RegisterPostInitializationOutput(static c => c.AddSource($"{nameof(UnionTypeAttribute)}.g.cs", UnionTypeAttribute.SourceText));
        context.RegisterPostInitializationOutput(static c => c.AddSource($"{nameof(RelationAttribute)}.g.cs", RelationAttribute.SourceText));
        context.RegisterPostInitializationOutput(static c => c.AddSource($"{nameof(UnionTypeSettingsAttribute)}.g.cs", UnionTypeSettingsAttribute.SourceText));
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
