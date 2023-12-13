namespace RhoMicro.CodeAnalysis.UnionsGenerator.Generators.Expansions;

using Microsoft.CodeAnalysis;

using RhoMicro.CodeAnalysis.UtilityGenerators.Library;
using RhoMicro.CodeAnalysis.UnionsGenerator.Models;

using System;
using System.Threading;

sealed class Match(TargetDataModel model) : ExpansionBase(model, Macro.Match)
{
    public override void Expand(ExpandingMacroBuilder builder)
    {
        var representableTypes = Model.Annotations.AllRepresentableTypes;
        var target = Model.Symbol;
        var settings = Model.Annotations.Settings;

        _ = builder.AppendLine("#region Match") *
            "public ").Append(settings.MatchTypeName).Append(" Match<").Append(settings.MatchTypeName).Append(">("
            .AppendJoin(
                representableTypes.Select((t, i) => (Type: t, Index: i)),
                (b, a, t) => b.Append("global::System.Func<")
                    .AppendFull(a.Type) *
                    ", "
                    .Append(settings.MatchTypeName) *
                    "> on"
                    .Append(a.Type.Names.SafeAlias)
                    .AppendLine(a.Index == representableTypes.Count - 1 ? String.Empty : ","),
                cancellationToken) /
            ") =>";

        builder = representableTypes.Count == 1
            ? builder.Append("on")
                .Append(representableTypes[0].Names.SafeAlias) *
                ".Invoke("
                .Append(representableTypes[0].Storage.TypesafeInstanceVariableExpressionAppendix, cancellationToken) /
                ");"
            : builder.AppendLine("__tag switch{")
                .AppendJoin(
                representableTypes,
                (b, a, t) => b.Append(a.CorrespondingTag) /
                    " => " *
                    "on").Append(a.Names.SafeAlias).Append(".Invoke("
                    .Append(a.Storage.TypesafeInstanceVariableExpressionAppendix, t).AppendLine("),"),
                cancellationToken) /
                "_ =>"
                .AppendLine(ConstantSources.InvalidTagStateThrow) /
                "};";

        _ = builder.AppendLine("#endregion");
    }
}
