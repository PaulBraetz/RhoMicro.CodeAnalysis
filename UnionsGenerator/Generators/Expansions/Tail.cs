namespace RhoMicro.CodeAnalysis.UnionsGenerator.Generators.Expansions;

using RhoMicro.CodeAnalysis.UtilityGenerators.Library;
using RhoMicro.CodeAnalysis.UnionsGenerator.Models;

sealed class Tail(TargetDataModel model)
    : ExpansionBase(model, Macro.Tail)
{
    public override void Expand(ExpandingMacroBuilder builder)
    {
        if(!Model.Symbol.ContainingNamespace.IsGlobalNamespace)
            _ = builder.AppendLine('}');

        _ = builder.AppendLine('}')
            .AppendLine(Model.Symbol.GetContainingClassTail());
    }
}
