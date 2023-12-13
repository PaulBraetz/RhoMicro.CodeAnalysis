namespace RhoMicro.CodeAnalysis.UnionsGenerator.Generators.Expansions;

using Microsoft.CodeAnalysis;

using RhoMicro.CodeAnalysis.UtilityGenerators.Library;
using RhoMicro.CodeAnalysis.UnionsGenerator.Models;

using System;
using System.Threading;

sealed class Switch(TargetDataModel model) : ExpansionBase(model, Macro.Switch)
{
    public override void Expand(ExpandingMacroBuilder builder)
    {
        var representableTypes = Model.Annotations.AllRepresentableTypes;
        var target = Model.Symbol;

        _ = builder.AppendLine("#region Switch")
            .AppendJoin(
                representableTypes,
                (b, a, t) => b.Append("/// <param name=\"on").Append(a.Names.SafeAlias) *
                "\">The handler to invoke if the union is currently representing an instance of "
                .AppendCommentRef(a).AppendLine(".</param>"),
                cancellationToken) *
            "public void Switch("
            .AppendJoin(
                representableTypes.Select((t, i) => (Type: t, Index: i)),
                (b, a, t) => b.Append("global::System.Action<")
                    .AppendFull(a.Type) *
                    "> on"
                    .Append(a.Type.Names.SafeAlias)
                    .AppendLine(a.Index == representableTypes.Count - 1 ? String.Empty : ","),
                cancellationToken) *
            "){";

        _ = (representableTypes.Count == 1
            ? builder.Append("on")
                .Append(representableTypes[0].Names.SafeAlias) *
                ".Invoke("
                .Append(representableTypes[0].Storage.TypesafeInstanceVariableExpressionAppendix, cancellationToken)
                .AppendLine(')')
            : builder.AppendLine("switch(__tag){")
                .AppendJoin(
                representableTypes,
                (b, a, t) => b.Append("case ")
                    .Append(a.CorrespondingTag)
                    .AppendLine(':') *
                    "on").Append(a.Names.SafeAlias).Append(".Invoke("
                    .Append(a.Storage.TypesafeInstanceVariableExpressionAppendix, t).AppendLine(");return;"),
                cancellationToken) /
                "default:"
                .Append(ConstantSources.InvalidTagStateThrow) *
                ";}") /
                ";}" /
                "#endregion";
    }
}
