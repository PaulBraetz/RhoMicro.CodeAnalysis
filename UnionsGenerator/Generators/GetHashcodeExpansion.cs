namespace RhoMicro.CodeAnalysis.UnionsGenerator.Generators;

using RhoMicro.CodeAnalysis.Common;
using RhoMicro.CodeAnalysis.UnionsGenerator.Models;

using System.Threading;

sealed class GetHashCodeExpansion(TargetDataModel model) : ExpansionBase(model, Macro.GetHashcode)
{
    public override void Expand(IExpandingMacroStringBuilder<Macro> builder, CancellationToken cancellationToken)
    {
        var annotations = Model.Annotations;
        var target = Model.Symbol;

        _ = builder.AppendLine("#region GetHashcode")
            .AppendLine("public override Int32 GetHashCode() => ");

        if(annotations.AllRepresentableTypes.Count > 1)
        {
            _ = builder.Append("__tag switch{")
                .AppendJoin(
                    annotations.AllRepresentableTypes,
                    (b, a, t) => b.Append(a.CorrespondingTag)
                        .Append(" => ")
                        .Append(a.Storage.GetHashCodeInvocationAppendix, t)
                        .AppendLine(','),
                    cancellationToken)
                .AppendLine("_ => ").Append(ConstantSources.InvalidTagStateThrow)
                .Append('}');

        } else if(annotations.AllRepresentableTypes.Count == 1)
        {
            var storage = annotations.AllRepresentableTypes[0].Storage;
            _ = builder.Append(storage.GetHashCodeInvocationAppendix, cancellationToken);
        }

        _ = builder.AppendLine(';').AppendLine("#endregion");
    }
}
