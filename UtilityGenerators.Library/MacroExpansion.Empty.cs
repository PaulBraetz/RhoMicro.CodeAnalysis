namespace RhoMicro.CodeAnalysis.Library;
static partial class MacroExpansion
{
    internal sealed class Empty<TMacro>(TMacro macro) : MacroExpansion<TMacro>(macro)
    {
        public override void Expand(IExpandingMacroStringBuilder<TMacro> builder, CancellationToken cancellationToken) { }
    }
}