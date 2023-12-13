namespace RhoMicro.CodeAnalysis.UtilityGenerators.Library;

using Microsoft.CodeAnalysis;

using System;

/// <summary>
/// Contains factory methods for generator contexts.
/// </summary>
static partial class GeneratorContext
{
    /// <summary>
    /// Creates a new generator context.
    /// </summary>
    /// <param name="model">The model based on which to generate source code and diagnostics.</param>
    /// <param name="configureSourceTextBuilder">
    /// The optional configuration to apply to the underlying source 
    /// text builder after its instantiation.
    /// </param>
    /// <param name="configureDiagnosticsAccumulator">
    /// The optional configuration to apply to the underlying diagnostics
    /// accumulator after its instantiation.
    /// </param>
    /// <returns>The new generator context.</returns>
    public static IGeneratorContext<TMacro, TModel> Create<TMacro, TModel>(
        TModel model,
        Func<IExpandingMacroStringBuilder<TMacro>, IExpandingMacroStringBuilder<TMacro>>? configureSourceTextBuilder = null,
        Func<IDiagnosticsAccumulator<TModel>, IDiagnosticsAccumulator<TModel>>? configureDiagnosticsAccumulator = null)
    {
        var diagnosticsAccumulator = DiagnosticsAccumulator.Create(model);
        if(configureDiagnosticsAccumulator != null)
        {
            diagnosticsAccumulator = configureDiagnosticsAccumulator.Invoke(diagnosticsAccumulator);
        }

        var builder = ExpandingMacroStringBuilder.Create<TMacro>()
            .InterceptErrors(diagnosticsAccumulator);
        if(configureSourceTextBuilder != null)
        {
            builder = configureSourceTextBuilder.Invoke(builder);
        }

        var result = new Impl<TMacro, TModel>(
            builder,
            diagnosticsAccumulator,
            model);

        return result;
    }

    public static readonly DiagnosticDescriptor BuildErrorDiagnosticDescriptor =
        new(
            "RCUL0001",
            "Failure While Building Source",
            "An unexpected error occured while building source text: {0}",
            "Source Generator",
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);
}
