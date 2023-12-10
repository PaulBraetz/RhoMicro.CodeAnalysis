namespace RhoMicro.CodeAnalysis.UnionsGenerator.Generators;

using Microsoft.CodeAnalysis;

using RhoMicro.CodeAnalysis.Common;
using RhoMicro.CodeAnalysis.UnionsGenerator.Models;

using System;
using System.Threading;

sealed class MatchExpansion(TargetDataModel model) : ExpansionBase(model, Macro.Match)
{
    public override void Expand(IExpandingMacroStringBuilder<Macro> builder, CancellationToken cancellationToken)
    {
        var representableTypes = Model.Annotations.AllRepresentableTypes;
        var target = Model.Symbol;
        var settings = Model.Annotations.Settings;

        _ = builder.AppendLine("#region Match")
            .Append("public ").Append(settings.MatchTypeName).Append(" Match<").Append(settings.MatchTypeName).Append(">(")
            .AppendJoin(
                representableTypes.Select((t, i) => (Type: t, Index: i)),
                (b, a, t) => b.Append("global::System.Func<")
                    .AppendFull(a.Type)
                    .Append(", ")
                    .Append(settings.MatchTypeName)
                    .Append("> on")
                    .Append(a.Type.Names.SafeAlias)
                    .AppendLine(a.Index == representableTypes.Count - 1 ? String.Empty : ","),
                cancellationToken)
            .AppendLine(") =>");

        builder = representableTypes.Count == 1
            ? builder.Append("on")
                .Append(representableTypes[0].Names.SafeAlias)
                .Append(".Invoke(")
                .Append(representableTypes[0].Storage.TypesafeInstanceVariableExpressionAppendix, cancellationToken)
                .AppendLine(");")
            : builder.AppendLine("__tag switch{")
                .AppendJoin(
                representableTypes,
                (b, a, t) => b.Append(a.CorrespondingTag)
                    .AppendLine(" => ")
                    .Append("on").Append(a.Names.SafeAlias).Append(".Invoke(")
                    .Append(a.Storage.TypesafeInstanceVariableExpressionAppendix, t).AppendLine("),"),
                cancellationToken)
                .AppendLine("_ =>")
                .AppendLine(ConstantSources.InvalidTagStateThrow)
                .AppendLine("};");

        _ = builder.AppendLine("#endregion");
    }
}
