namespace RhoMicro.CodeAnalysis.UnionsGenerator.Generators;

using Microsoft.CodeAnalysis;

using RhoMicro.CodeAnalysis.Common;
using RhoMicro.CodeAnalysis.UnionsGenerator.Models;

using System;
using System.Threading;

sealed class SwitchExpansion(TargetDataModel model) : ExpansionBase(model, Macro.Switch)
{
    public override void Expand(IExpandingMacroStringBuilder<Macro> builder, CancellationToken cancellationToken)
    {
        var representableTypes = Model.Annotations.AllRepresentableTypes;
        var target = Model.Symbol;

        _ = builder.AppendLine("#region Switch")
            .AppendLine("/// <summary>")
            .AppendLine("/// <inheritdoc/>")
            .AppendLine("/// <summary>")
            .AppendJoin(
                representableTypes,
                (b, a, t) => b.Append("/// <param name=\"on").Append(a.Names.SafeAlias)
                .Append("\">The handler to invoke if the union is currently representing an instance of ")
                .AppendCommentRef(a).AppendLine(".</param>"),
                cancellationToken)
            .Append("public void Switch(")
            .AppendJoin(
                representableTypes.Select((t, i) => (Type: t, Index: i)),
                (b, a, t) => b.Append("global::System.Action<")
                    .AppendFull(a.Type)
                    .Append("> on")
                    .Append(a.Type.Names.SafeAlias)
                    .AppendLine(a.Index == representableTypes.Count - 1 ? String.Empty : ","),
                cancellationToken)
            .Append("){");

        _ = (representableTypes.Count == 1
            ? builder.Append("on")
                .Append(representableTypes[0].Names.SafeAlias)
                .Append(".Invoke(")
                .Append(representableTypes[0].Storage.TypesafeInstanceVariableExpressionAppendix, cancellationToken)
                .AppendLine(')')
            : builder.AppendLine("switch(__tag){")
                .AppendJoin(
                representableTypes,
                (b, a, t) => b.Append("case ")
                    .Append(a.CorrespondingTag)
                    .AppendLine(':')
                    .Append("on").Append(a.Names.SafeAlias).Append(".Invoke(")
                    .Append(a.Storage.TypesafeInstanceVariableExpressionAppendix, t).AppendLine(");return;"),
                cancellationToken)
                .AppendLine("default:")
                .Append(ConstantSources.InvalidTagStateThrow)).AppendLine(";}}").AppendLine("#endregion");
    }
}
