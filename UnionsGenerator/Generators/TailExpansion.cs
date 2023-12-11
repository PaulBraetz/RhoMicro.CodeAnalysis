namespace RhoMicro.CodeAnalysis.UnionsGenerator.Generators;

using RhoMicro.CodeAnalysis.UtilityGenerators.Library;
using RhoMicro.CodeAnalysis.UnionsGenerator.Models;

sealed class TailExpansion(TargetDataModel model)
    : ExpansionBase(model, Macro.Tail)
{
    public override void Expand(IExpandingMacroStringBuilder<Macro> builder, CancellationToken cancellationToken)
    {
        if(!Model.Symbol.ContainingNamespace.IsGlobalNamespace)
            _ = builder.AppendLine('}');

        _ = builder.AppendLine('}')
            .AppendLine(Model.Symbol.GetContainingClassTail());
    }
}
