namespace RhoMicro.CodeAnalysis.UnionsGenerator.Generators;

using RhoMicro.CodeAnalysis.UtilityGenerators.Library;
using RhoMicro.CodeAnalysis.UnionsGenerator.Models;

using System.Threading;

sealed class DownCastExpansion(TargetDataModel model) : ExpansionBase(model, Macro.DownCast)
{
    public override void Expand(IExpandingMacroStringBuilder<Macro> builder, CancellationToken cancellationToken)
    {
        var representableTypes = Model.Annotations.AllRepresentableTypes;
        var target = Model.Symbol;
        var settings = Model.Annotations.Settings;

        _ = builder.AppendLine("#region DownCast")
            .AppendLine("/// </inheritdoc>")
            .Append("public ")
            .Append(settings.MatchTypeName)
            .Append(" DownCast<")
            .Append(settings.MatchTypeName)
            .Append(">()")
            .AppendLine(" where ")
            .Append(settings.MatchTypeName)
            .Append(" : global::RhoMicro.CodeAnalysis.UnionsGenerator.Abstractions.IUnion<")
            .Append(settings.MatchTypeName)
            .Append(',')
            .AppendJoin(
                ",",
                representableTypes,
                (b, a, t) => b.AppendFull(a))
            .Append('>');

#pragma warning disable IDE0045 // Convert to conditional expression
        if(representableTypes.Count == 1)
        {
            _ = builder.Append(" => ")
                .Append(settings.MatchTypeName)
                .Append(".Create<").AppendFull(representableTypes[0])
                .Append(">(").Append(representableTypes[0].Storage.TypesafeInstanceVariableExpressionAppendix, cancellationToken).Append(')');
        } else
        {
            _ = builder.AppendLine(" => __tag switch{")
                .AppendJoin(
                representableTypes,
                (b, a, t) => b.Append(a.CorrespondingTag)
                    .Append(" => ")
                    .Append(settings.MatchTypeName)
                    .Append(".Create<").AppendFull(a)
                    .Append(">(").Append(a.Storage.TypesafeInstanceVariableExpressionAppendix, cancellationToken).Append(')')
                    .AppendLine(','),
                cancellationToken)
                .Append("_ => ")
                .Append(ConstantSources.InvalidTagStateThrow)
                .Append('}');
        }
#pragma warning restore IDE0045 // Convert to conditional expression

        _ = builder.AppendLine(';').AppendLine("#endregion");
    }
}
