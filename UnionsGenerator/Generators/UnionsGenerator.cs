namespace RhoMicro.CodeAnalysis.UnionsGenerator.Generators;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using RhoMicro.CodeAnalysis;
using RhoMicro.CodeAnalysis.Common;
using RhoMicro.CodeAnalysis.UnionsGenerator.Analyzers;
using RhoMicro.CodeAnalysis.UnionsGenerator.Models;

using System;
using System.Text;
using System.Threading;

enum Macro
{
    Head,
    NestedClasses,
    Constructors,
    Fields,
    Factories,
    DownCast,
    Switch,
    Match,
    RepresentedTypes,
    IsAs,
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
                    configureSourceTextBuilder: b => b
                        .Receive(new HeadExpansion(model))
                        .Receive(new NestedTypesExpansion(model))
                        .Receive(new ConstructorsExpansion(model))
                        .Receive(new FieldsExpansion(model))
                        .Receive(new FactoriesExpansion(model))
                        .Receive(new TailExpansion(model))
                        .Receive(new DownCastExpansion(model))
                        .Receive(new RepresentedTypesExpansion(model))
                        .Receive(new IsAsExpansion(model))
                        .Receive(new MatchExpansion(model))
                        .Receive(new SwitchExpansion(model))
                        .Receive(new GetHashCodeExpansion(model))
                        .Receive(new EqualsExpansion(model))
                        .Receive(ToStringExpansion.Create(model))
                        .Receive(ConversionExpansion.Create(model))
                        .AppendHeader("RhoMicro.CodeAnalysis.UnionsGenerator")
                        .AppendMacros(t)
                        .Format(),
                    configureDiagnosticsAccumulator: d => d
                        .ReportNonHiddenSeverities()
                        .DiagnoseNonHiddenSeverities()
                        .Receive(Providers.All, t));

                var source = context.BuildSource(t);
                var hintName = $"{model.Symbol.ToHintName()}.g.cs";

                return (source, hintName);
            });

        context.RegisterSourceOutput(providers, static (c, t) => c.AddSource(t.hintName, t.source));
        context.RegisterPostInitializationOutput(static c => c.AddSource("Util.g.cs", ConstantSources.Util));
        context.RegisterPostInitializationOutput(static c => c.AddSource($"{nameof(UnionTypeAttribute)}.g.cs", UnionTypeAttribute.SourceText));
        context.RegisterPostInitializationOutput(static c => c.AddSource($"{nameof(RelationAttribute)}.g.cs", RelationAttribute.SourceText));
        context.RegisterPostInitializationOutput(static c => c.AddSource($"{nameof(UnionTypeSettingsAttribute)}.g.cs", UnionTypeSettingsAttribute.SourceText));
    }
}
