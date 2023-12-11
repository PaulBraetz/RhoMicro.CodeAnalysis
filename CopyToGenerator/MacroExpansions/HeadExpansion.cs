namespace RhoMicro.CodeAnalysis.CopyToGenerator.ExpansionProviders;

using RhoMicro.CodeAnalysis.UtilityGenerators.Library;
using RhoMicro.CodeAnalysis.CopyToGenerator;
using RhoMicro.CodeAnalysis.CopyToGenerator.MacroExpansions;

sealed class HeadExpansion(Model model) : MacroExpansionBase(model, Macro.Head)
{
    public override void Expand(IExpandingMacroStringBuilder<Macro> b, CancellationToken cancellationToken)
    {
        if(!Model.IsInGlobalNamespace)
        {
            _ = b.Append("namespace ")
                .AppendLine(Model.Namespace)
                .Append('{');
        }

        _ = b
            .Append("partial class ").AppendLine(Model.Name)
            .Append('{');
    }
}
