namespace RhoMicro.CodeAnalysis.UnionsGenerator.Generators;

using RhoMicro.CodeAnalysis.Common;
using RhoMicro.CodeAnalysis.UnionsGenerator.Models;

using System.Threading;

sealed class ConstructorsExpansion(TargetDataModel model) : ExpansionBase(model, Macro.Constructors)
{
    public override void Expand(IExpandingMacroStringBuilder<Macro> builder, CancellationToken cancellationToken)
    {
        var target = Model.Symbol;
        var annotations = Model.Annotations;

        _ = builder.AppendLine("#region Constructors")
            .AppendJoin(annotations.AllRepresentableTypes, (b, e, t) =>
                {
                    var accessibility = Model.GetSpecificAccessibility(e);

                    _ = b.Append(accessibility)
                        .Append(' ')
                        .Append(target.Name)
                        .Append('(')
                        .AppendFull(e)
                        .AppendLine(" value){");

                    if(annotations.AllRepresentableTypes.Count > 1)
                        _ = b.Append("__tag = ").Append(e.CorrespondingTag).AppendLine(';');
                    var result = b.Append(
                        e.Storage.InstanceVariableAssignmentExpressionAppendix,
                        ("value", "this")
                        , cancellationToken)
                        .AppendLine(";}");

                    return result;
                }, cancellationToken).AppendLine("#endregion");
    }
}
