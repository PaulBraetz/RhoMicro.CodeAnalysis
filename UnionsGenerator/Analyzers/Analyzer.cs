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
        _ = context ?? throw new ArgumentNullException(nameof(context));

        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSymbolStartAction(ctx =>
        {
            ctx.CancellationToken.ThrowIfCancellationRequested();
            if(ctx is not { IsGeneratedCode: false, Symbol: INamedTypeSymbol target } || !Util.IsUnionTypeSymbol(target, ctx.CancellationToken))
                return;

            ctx.RegisterSymbolEndAction(ctx =>
            {
                ctx.CancellationToken.ThrowIfCancellationRequested();
                if(ctx is not { IsGeneratedCode: false, Symbol: INamedTypeSymbol target } || !Util.IsUnionTypeSymbol(target, ctx.CancellationToken))
                    return;

                var model = UnionTypeModel.Create(target, ctx.CancellationToken);

                var accumulator = DiagnosticsAccumulator.Create(model)
                                .DiagnoseNonHiddenSeverities()
                                .ReportNonHiddenSeverities()
                                .Receive(Providers.All, ctx.CancellationToken);

                accumulator.ReportDiagnostics(ctx.ReportDiagnostic);
            });
        }, SymbolKind.NamedType);
    }

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = Diagnostics.Descriptors.ToImmutableArray();
}