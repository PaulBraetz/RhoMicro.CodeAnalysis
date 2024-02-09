namespace RhoMicro.CodeAnalysis.UnionsGenerator.Generators.Expansions;

using RhoMicro.CodeAnalysis.Library;
using RhoMicro.CodeAnalysis.UnionsGenerator.Models;

internal sealed class Tail(TargetDataModel model)
    : ExpansionBase(model, Macro.Tail)
{
    protected override void Expand(ExpandingMacroBuilder builder)
    {
        if(!Model.Symbol.ContainingNamespace.IsGlobalNamespace)
            _ = builder.AppendLine('}');

        _ = builder.AppendLine('}');

        var containingType = Model.Symbol.ContainingType;
        while(containingType != null)
        {
            _ = builder.AppendLine('}');

            containingType = containingType.ContainingType;
        }
    }
}
