#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace RhoMicro.CodeAnalysis.UnionsGenerator.Analyzers;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using RhoMicro.CodeAnalysis.Library;
using RhoMicro.CodeAnalysis.UnionsGenerator.Models;
using RhoMicro.CodeAnalysis.UnionsGenerator.Utils;

using System.Collections.Immutable;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class Analyzer : DiagnosticAnalyzer
{
    public override void Initialize(AnalysisContext context)
    {
        _ = context ?? throw new ArgumentNullException(nameof(context));

        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSymbolStartAction(ctx =>
        {
            ctx.CancellationToken.ThrowIfCancellationRequested();
            if(ctx is not { IsGeneratedCode: false, Symbol: INamedTypeSymbol target } || !Qualifications.IsUnionTypeSymbol(target, ctx.CancellationToken))
                return;

            ctx.RegisterSymbolEndAction(ctx =>
            {
                ctx.CancellationToken.ThrowIfCancellationRequested();
                if(ctx is not { IsGeneratedCode: false, Symbol: INamedTypeSymbol target } || !Qualifications.IsUnionTypeSymbol(target, ctx.CancellationToken))
                    return;

                var model = UnionTypeModel.Create(target, ctx.CancellationToken);

                if(model.Settings.DiagnosticsLevel == DiagnosticsLevelSettings.None)
                    return;

                var accumulator = DiagnosticsAccumulator.Create(model)
                    .DiagnoseNonHiddenSeverities();

                if(model.Settings.DiagnosticsLevel != DiagnosticsLevelSettings.None)
                {
                    if(model.Settings.DiagnosticsLevel.HasFlag(DiagnosticsLevelSettings.Info))
                    {
                        accumulator = accumulator.ReportSeverity(DiagnosticSeverity.Info);
                    }

                    if(model.Settings.DiagnosticsLevel.HasFlag(DiagnosticsLevelSettings.Warning))
                    {
                        accumulator = accumulator.ReportSeverity(DiagnosticSeverity.Warning);
                    }

                    if(model.Settings.DiagnosticsLevel.HasFlag(DiagnosticsLevelSettings.Error))
                    {
                        accumulator = accumulator.ReportSeverity(DiagnosticSeverity.Error);
                    }
                }

                accumulator.Receive(Providers.All, ctx.CancellationToken)
                    .ReportDiagnostics(ctx.ReportDiagnostic);
            });
        }, SymbolKind.NamedType);
    }

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = Diagnostics.Descriptors.ToImmutableArray();
}