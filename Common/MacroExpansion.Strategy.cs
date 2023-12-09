namespace RhoMicro.CodeAnalysis.Common;

partial class MacroExpansion
{
    /// <summary>
    /// Imlements a strategy-based expansion provider.
    /// </summary>
    /// <typeparam name="TMacro">The type of macro to provide expansions for.</typeparam>
    /// <param name="macro">The macro to replace.</param>
    /// <param name="strategy">The strategy to use when replacing the macro.</param>
    internal sealed class Strategy<TMacro>(
        TMacro macro,
        Action<IExpandingMacroStringBuilder<TMacro>, CancellationToken> strategy) :
        MacroExpansion<TMacro>(macro)
    {
        private readonly Action<IExpandingMacroStringBuilder<TMacro>, CancellationToken> _strategy = strategy;
        /// <inheritdoc/>
        public override void Expand(
            IExpandingMacroStringBuilder<TMacro> builder,
            CancellationToken cancellationToken) =>
            _strategy.Invoke(builder, cancellationToken);
    }
}