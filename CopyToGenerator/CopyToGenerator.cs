#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace RhoMicro.CodeAnalysis.CopyToGenerator;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using RhoMicro.CodeAnalysis.CopyToGenerator.ExpansionProviders;
using RhoMicro.CodeAnalysis.UtilityGenerators.Library;

using System;
using System.Linq;

[Generator]
public class CopyToGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var provider = context.SyntaxProvider
            .ForGenerateCopyToAttribute(
                static (n, t) => n is ClassDeclarationSyntax c && c.Modifiers.Any(SyntaxKind.PartialKeyword),
                static (c, t) =>
                {
                    var target = c.TargetSymbol as INamedTypeSymbol ??
                        throw new InvalidOperationException("Unable to retrieve target symbol as named type symbol.");

                    var model = Model.Create(target);
                    return model;
                })
            .Select(static (m, t) => GeneratorContext.Create<Macro, Model>(
                m,
                configureSourceTextBuilder: b => b
                    .Receive(new HeadExpansion(m), t)
                    .Receive(new AvoidCopyExpansion(m), t)
                    .Receive(new CopyToMethodExpansion(m), t)
                    .Receive(new TailExpansion(m), t)
                    .AppendHeader("RhoMicro.CodeAnalysis.CopyToGenerator")
                    .AppendMacros(t)
                    .Format()))
            .Select(static (c, t) => (Source: c.BuildSource(t), HintName: $"{c.Model.Name}.g.cs"));

        context.RegisterSourceOutput(provider, (c, output) => output.Source.AddToContext(c, output.HintName));
        context.RegisterPostInitializationOutput(c => c.AddSource($"{nameof(GenerateCopyToAttribute)}.g.cs", GenerateCopyToAttribute.SourceText));
    }
}
