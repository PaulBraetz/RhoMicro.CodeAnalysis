namespace RhoMicro.CodeAnalysis.Library;

using System;

/// <summary>
/// Thrown if an infinite macro expansion is detected.
/// </summary>
/// <typeparam name="TMacro">The type of macro to be expanded into a sequence containing itself.</typeparam>
/// <param name="macro">The macro to be expanded into a sequence containing itself.</param>
sealed class InfinitelyRecursingExpansionException<TMacro>(TMacro macro)
    : Exception($"Detected infinitely recursing macro expansion while expanding '{macro}'. Make sure that macro expansions do not expand into themselves.")
{
    /// <summary>
    /// Gets the macro to be expanded into a sequence containing itself.
    /// </summary>
    public TMacro Macro { get; } = macro;
}
