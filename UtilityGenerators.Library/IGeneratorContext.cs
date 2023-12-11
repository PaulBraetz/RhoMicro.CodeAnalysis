namespace RhoMicro.CodeAnalysis.UtilityGenerators.Library;

using System;

/// <summary>
/// Represents the generator context for incremental generators. 
/// Appending or substituting values to the underlying source text 
/// builder are intercepted and discarded if the underlying
/// diagnostics accumulator is currently reporting errors.
/// This means that the expanding macro string builder provided by context 
/// instances will essentially short circuit upon errors having been 
/// reported by the diagnostics accumulator.
/// </summary>
/// <typeparam name="TMacro">The type of macro to support.</typeparam>
/// <typeparam name="TModel">The type of model to diagnose.</typeparam>
interface IGeneratorContext<TMacro, TModel>
{
    /// <summary>
    /// Gets the model to incrementally build up.
    /// </summary>
    TModel Model { get; }

    /// <summary>
    /// Applies a modification to the diagnostics accumulator underlying the context.
    /// </summary>
    /// <param name="diagnose">The modification to apply to the underlying diagnostics accumulator.</param>
    /// <param name="cancellationToken">A token signalling the diagnosis should terminate.</param>
    /// <returns>A reference to the context, for chaining of further method calls.</returns>
    IGeneratorContext<TMacro, TModel> ApplyToDiagnostics(Action<IDiagnosticsAccumulator<TModel>, TModel, CancellationToken> diagnose, CancellationToken cancellationToken);
    /// <summary>
    /// Applies a transformation to the diagnostics accumulator underlying the context.
    /// </summary>
    /// <param name="diagnose">The transformation to apply to the underlying diagnostics accumulator.</param>
    /// <param name="cancellationToken">A token signalling the diagnosis should terminate.</param>
    /// <returns>A reference to the context, for chaining of further method calls.</returns>
    IGeneratorContext<TMacro, TModel> ApplyToDiagnostics(Func<IDiagnosticsAccumulator<TModel>, TModel, CancellationToken, IDiagnosticsAccumulator<TModel>> diagnose, CancellationToken cancellationToken);
    /// <summary>
    /// Applies a modification to the string builder underlying the context.
    /// </summary>
    /// <param name="build">The modification to apply to the underlying string builder.</param>
    /// <param name="cancellationToken">The cancellation token used to signal expansion to cancel.</param>
    /// <returns>A reference to the context, for chaining of further method calls.</returns>
    IGeneratorContext<TMacro, TModel> ApplyToSource(Action<IExpandingMacroStringBuilder<TMacro>, TModel, CancellationToken> build, CancellationToken cancellationToken);
    /// <summary>
    /// Applies a transformation to the string builder underlying the context.
    /// </summary>
    /// <param name="build">The transformation to apply to the underlying string builder.</param>
    /// <param name="cancellationToken">The cancellation token used to signal expansion to cancel.</param>
    /// <returns>A reference to the context, for chaining of further method calls.</returns>
    IGeneratorContext<TMacro, TModel> ApplyToSource(Func<IExpandingMacroStringBuilder<TMacro>, TModel, CancellationToken, IExpandingMacroStringBuilder<TMacro>> build, CancellationToken cancellationToken);

    /// <summary>
    /// Receives a new expansion and diagnostics provider of the type provided, 
    /// to be received by both the underlying diagnostics accumulator, as well 
    /// as the underlying source text builder.
    /// </summary>
    /// <typeparam name="TProvider">The type of provider to receive.</typeparam>
    /// <returns>A reference to the context, for chaining of further method calls.</returns>
    IGeneratorContext<TMacro, TModel> Receive<TProvider>() where TProvider : IMacroExpansion<TMacro>, IDiagnosticProvider<TModel>, new();
    /// <summary>
    /// Receives a expansion and diagnostics provider, to be received by both 
    /// the underlying diagnostics accumulator, as well as the underlying 
    /// source text builder.
    /// </summary>
    /// <typeparam name="TProvider">The type of provider to receive.</typeparam>
    /// <param name="provider">The provider to receive.</param>
    /// <returns>A reference to the context, for chaining of further method calls.</returns>
    IGeneratorContext<TMacro, TModel> Receive<TProvider>(TProvider provider) where TProvider : IMacroExpansion<TMacro>, IDiagnosticProvider<TModel>;

    /// <summary>
    /// Builds the source text.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token used to signal source text generation to cancel.</param>
    /// <returns>The built source code.</returns>
    String BuildSource(CancellationToken cancellationToken);
}
