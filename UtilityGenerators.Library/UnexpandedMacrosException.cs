namespace RhoMicro.CodeAnalysis.UtilityGenerators.Library;

using System;

/// <summary>
/// Thrown if <see cref="IExpandingMacroStringBuilder{TMacro}.Build(CancellationToken)"/> has been called but there are unexpanded macros present in the builder.
/// </summary>
/// <typeparam name="TMacro">The type of unexpanded macro.</typeparam>
/// <param name="macros">The list of unexpanded macros.</param>
sealed class UnexpandedMacrosException<TMacro>(IReadOnlyList<TMacro> macros) 
    : Exception($"Unable to build string; there is at least one unexpanded macro present in the builder. Make sure to provide expansions for all macros in use. Unexpanded macros: {String.Join(", ", macros)}")
{
    /// <summary>
    /// Gets the unexpanded macros.
    /// </summary>
    public IReadOnlyList<TMacro> Macros { get; } = macros;
}