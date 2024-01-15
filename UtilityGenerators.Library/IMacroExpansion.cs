namespace RhoMicro.CodeAnalysis.Library;

/// <summary>
/// Expands macro instances into values and/or further macros.
/// </summary>
/// <typeparam name="TMacro">The type of macro to expand.</typeparam>
interface IMacroExpansion<TMacro>
{
    /// <summary>
    /// Gets the macro instance to expand.
    /// </summary>
    TMacro Macro { get; }
    /// <summary>
    /// Expands the macro into an expanding macro string builder.
    /// </summary>
    /// <param name="builder">The builder to expand the macro into.</param>
    /// <param name="cancellationToken">The cancellation token used to signal expansion to cancel.</param>
    void Expand(IExpandingMacroStringBuilder<TMacro> builder, CancellationToken cancellationToken);
}
