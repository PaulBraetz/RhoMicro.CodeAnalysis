namespace RhoMicro.CodeAnalysis.UtilityGenerators.Library;

/// <summary>
/// Represents a builder for incrementally constructing strings based on values and macros.
/// Macros must be expanded into values and/or further macros. 
/// The builder may only build the final result once there are no more macros to expand.
/// </summary>
/// <typeparam name="TMacro">The type of macro to support.</typeparam>
interface IExpandingMacroStringBuilder<TMacro>
{
    /// <summary>
    /// Builds the result string.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token used to signal expansion to cancel.</param>
    /// <returns>The result string.</returns>
    String Build(CancellationToken cancellationToken = default);
    /// <summary>
    /// Appends a string value to the builder.
    /// </summary>
    /// <param name="value">The string value to append.</param>
    /// <returns>A reference to the builder, for chaining of further methods.</returns>
    IExpandingMacroStringBuilder<TMacro> Append(String value);
    /// <summary>
    /// Appends a string value to the builder.
    /// </summary>
    /// <param name="value">The character value to append.</param>
    /// <returns>A reference to the builder, for chaining of further methods.</returns>
    IExpandingMacroStringBuilder<TMacro> Append(Char value);
    /// <summary>
    /// Appends a macro to the builder.
    /// </summary>
    /// <param name="macro">The macro to append.</param>
    /// <param name="cancellationToken">The cancellation token used to signal expansion to cancel.</param>
    /// <returns>A reference to the builder, for chaining of further methods.</returns>
    IExpandingMacroStringBuilder<TMacro> AppendMacro(TMacro macro, CancellationToken cancellationToken = default);
    /// <summary>
    /// Registers a expansion provider to the builder and potentially executes macro expansions.
    /// </summary>
    /// <param name="provider">The provider to register.</param>
    /// <param name="cancellationToken">The cancellation token used to signal expansion to cancel.</param>
    /// <returns>A reference to the builder, for chaining of further methods.</returns>
    IExpandingMacroStringBuilder<TMacro> Receive(IMacroExpansion<TMacro> provider, CancellationToken cancellationToken = default);
}
