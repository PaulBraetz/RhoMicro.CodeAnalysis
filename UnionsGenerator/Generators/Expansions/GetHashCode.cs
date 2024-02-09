﻿namespace RhoMicro.CodeAnalysis.UnionsGenerator.Generators.Expansions;

using RhoMicro.CodeAnalysis.Library;
using RhoMicro.CodeAnalysis.UnionsGenerator.Models;
using RhoMicro.CodeAnalysis.UnionsGenerator.Utils;

internal sealed class GetHashCode(TargetDataModel model) : ExpansionBase(model, Macro.GetHashcode)
{
    protected override void Expand(ExpandingMacroBuilder builder)
    {
        if(Model.ImplementsGetHashCode)
        {
            return;
        }

        var annotations = Model.Annotations;
        var target = Model.Symbol;

        _ = builder * "#region GetHashcode" /
            "public override Int32 GetHashCode() => ";

        if(annotations.AllRepresentableTypes.Count > 1)
        {
            _ = builder * "__tag switch{" *
                (b => b.AppendJoin(
                    annotations.AllRepresentableTypes,
                    (b, a, t) => _ = b.WithOperators(builder.CancellationToken) *
                        a.GetCorrespondingTag(Model) * " => " * a.Storage.GetHashCodeInvocation * ',',
                    b.CancellationToken)) /
                "_ => " * ConstantSources.InvalidTagStateThrow * '}';

        } else if(annotations.AllRepresentableTypes.Count == 1)
        {
            _ = builder * annotations.AllRepresentableTypes[0].Storage.GetHashCodeInvocation;
        }

        _ = builder %
            ';' %
            "#endregion";
    }
}
