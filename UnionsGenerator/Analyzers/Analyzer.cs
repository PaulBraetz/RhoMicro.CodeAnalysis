#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace RhoMicro.CodeAnalysis.UnionsGenerator.Analyzers;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using RhoMicro.CodeAnalysis.Library;
using RhoMicro.CodeAnalysis.UnionsGenerator.Models;

using System.Collections.Immutable;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Analyzer : DiagnosticAnalyzer
{
    public override void Initialize(AnalysisContext context)
    {
        //commented out because of breaking rewrite changes
        return;
        _ = context ?? throw new ArgumentNullException(nameof(context));

        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.Analyze | GeneratedCodeAnalysisFlags.ReportDiagnostics);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(static c =>
        {
            if(c.IsGeneratedCode || !Util.IsAnalysisCandidate(c.Node, c.SemanticModel))
                return;

            var model = TargetDataModel.Create((TypeDeclarationSyntax)c.Node, c.SemanticModel);

            var accumulator = DiagnosticsAccumulator.Create(model)
                            .DiagnoseNonHiddenSeverities()
                            .ReportNonHiddenSeverities()
                            .Receive(Providers.All, c.CancellationToken);

            accumulator.ReportDiagnostics(c.ReportDiagnostic);
        }, SyntaxKind.ClassDeclaration, SyntaxKind.StructDeclaration, SyntaxKind.RecordDeclaration, SyntaxKind.RecordStructDeclaration);
    }

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = Diagnostics.Descriptors.ToImmutableArray();
}