namespace RhoMicro.CodeAnalysis.UnionsGenerator;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using System.Collections.Generic;
using System;
using System.Collections.Immutable;

[Generator(LanguageNames.CSharp)]
public sealed class Analyzer : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var handledTargets = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);
        var models = context.SyntaxProvider.CreateSyntaxProvider(
            Extensions.IsUnionDeclaration,
            (c, t) =>
            {
                var parameters = TargetDataModel.Create((TypeDeclarationSyntax)c.Node, c.SemanticModel);
                var result = new DiagnosticsModelBuilder()
                                .ReportDiagnostics()
                                .Diagnose(parameters, t)
                                .Build(parameters.Annotations.Settings);

                return result;
            });

        context.RegisterSourceOutput(models, (c, sc) => sc.AddToContext(c));
    }
}

//[DiagnosticAnalyzer(LanguageNames.CSharp)]
//public sealed class Analyzer : DiagnosticAnalyzer
//{
//    public override void Initialize(AnalysisContext context)
//    {
//        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
//        context.EnableConcurrentExecution();
//        context.RegisterSyntaxNodeAction(c =>
//        {
//            if(c.IsGeneratedCode || c.Node is not TypeDeclarationSyntax targetDeclaration)
//                return;

//            var parameters = ModelFactoryParameters.Create(targetDeclaration, c.SemanticModel);

//            new DiagnosticsModelBuilder()
//                .Diagnose(parameters, c.CancellationToken)
//                .ReportDiagnostics()
//                .Build()
//                .AddToContext(c);
//        }, SyntaxKind.ClassDeclaration, SyntaxKind.StructDeclaration);
//    }

//    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; }
//}
