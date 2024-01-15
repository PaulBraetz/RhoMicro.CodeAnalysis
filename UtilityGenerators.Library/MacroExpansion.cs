namespace RhoMicro.CodeAnalysis.Library;

/// <summary>
/// Provides a base class for types implementing <see cref="IMacroExpansion{TMacro}"/>.
/// </summary>
/// <typeparam name="TMacro">The type of macro to provide expansions for.</typeparam>
/// <param name="macro">The macro to expand using this provider.</param>
abstract class MacroExpansion<TMacro>(TMacro macro) : IMacroExpansion<TMacro>
{
    /// <inheritdoc/>
    public TMacro Macro { get; } = macro;
    /// <inheritdoc/>
    public abstract void Expand(
        IExpandingMacroStringBuilder<TMacro> builder,
        CancellationToken cancellationToken);
}
/// <summary>
/// Provides factory and extension methods for macro expansion providers.
/// </summary>
static partial class MacroExpansion
{
    /// <summary>
    /// Creates a new empty macro expansion for the macro provided.
    /// </summary>
    /// <typeparam name="TMacro">The type of macro to provide an empty expansion for.</typeparam>
    /// <param name="macro">The macro to expand using the provider created.</param>
    /// <returns>A new empty macro expansion.</returns>
    public static IMacroExpansion<TMacro> CreateEmpty<TMacro>(TMacro macro) => new Empty<TMacro>(macro);
    /// <summary>
    /// Creates a new macro expansion provider.
    /// </summary>
    /// <typeparam name="TMacro">The type of macro to provide expansions for.</typeparam>
    /// <param name="macro">The macro to expand using the provider created.</param>
    /// <param name="strategy">The strategy using which to supply an expansion to an expanding macro string builder.</param>
    /// <returns>A new expansion.</returns>
    public static IMacroExpansion<TMacro> Create<TMacro>(
        TMacro macro,
        Action<IExpandingMacroStringBuilder<TMacro>, CancellationToken> strategy) =>
        new Strategy<TMacro>(macro, strategy);
}