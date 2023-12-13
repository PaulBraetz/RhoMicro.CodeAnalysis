using RhoMicro.CodeAnalysis.UnionsGenerator.Generators.Expansions;

namespace RhoMicro.CodeAnalysis.UnionsGenerator.Generators.Expansions;

using RhoMicro.CodeAnalysis.UnionsGenerator.Generators.Expansions;
using RhoMicro.CodeAnalysis.UnionsGenerator.Models;
using RhoMicro.CodeAnalysis.UtilityGenerators.Library;

using System;
using System.Collections.Generic;
using System.Runtime;
using System.Text;
using System.Threading;
using System.Xml.Linq;

sealed class IsAsFunctions(TargetDataModel model) : ExpansionBase(model, Macro.IsAsFunctions)
{
    public override void Expand(ExpandingMacroBuilder builder)
    {
        var attributes = Model.Annotations.AllRepresentableTypes;
        var target = Model.Symbol;
        var settings = Model.Annotations.Settings;

        _ = builder.AppendLine("#region IsAsFunctions") *
            "public global::System.Boolean Is<"
            .Append(settings.GenericTValueName) *
            ">() => ";
#pragma warning disable IDE0045 // Convert to conditional expression
        if(attributes.Count > 1)
        {
            _ = builder.AppendLine("typeof(")
                .Append(settings.GenericTValueName) *
                ") == __tag switch {"
                .AppendJoin(
                    attributes,
                    (b, a, t) => b.Append(a.CorrespondingTag).Append(" => typeof(").AppendFull(a).AppendLine("),"),
                    cancellationToken) /
                "_ => ".Append(ConstantSources.InvalidTagStateThrow) /
                "};";
        } else
        {
            _ = builder.Append("typeof(")
                .Append(settings.GenericTValueName) *
                ") == typeof("
                .AppendFull(attributes[0]) /
                ");";
        }

        _ = builder.Append("/// <summary>Determines whether this instance is representing a value of type <typeparamref name=\"")
            .Append(settings.GenericTValueName).AppendLine("\"/>.</summary>") *
            "/// <typeparam name=\"").Append(settings.GenericTValueName).AppendLine("\">The type whose representation in this instance to determine.</typeparam>" *
            "/// <param name=\"value\">If this instance is representing a value of type <typeparamref name=\"" /
            "\"/>, this parameter will contain that value; otherwise, <see langword=\"default\"/>.</param>" *
            "/// <returns><see langword=\"true\"/> if this instance is representing a value of type <typeparamref name=\""
            .Append(settings.GenericTValueName).AppendLine("\"/>; otherwise, <see langword=\"false\"/>.</returns>") *
            "public global::System.Boolean Is<").Append(settings.GenericTValueName).Append(">(out "
            .Append(settings.GenericTValueName).AppendLine("? value){");

        if(attributes.Count > 1)
        {
            _ = builder
                .AppendSwitchStatement(
                attributes,
                (b, t) => b.Append("__tag"),
                (b, a, t) => b.Append(a.CorrespondingTag) *
                    " when typeof(").Append(settings.GenericTValueName).Append(") == typeof("
                    .Append(a.Names.FullTypeName).Append(')'),
                (b, a, t) => b.Append("value = ").Append(a.Storage.ConvertedInstanceVariableExpressionAppendix, settings.GenericTValueName, t).AppendLine(';') *
                    "return true;",
                (b, t) => b.Append("value = default; return false;"),
                cancellationToken) /
                "}";
        } else
        {
            _ = builder.Append("if(typeof(").Append(settings.GenericTValueName).Append(") == typeof(")
                .Append(attributes[0].Names.FullTypeName).Append(")){value = ")
                .Append(attributes[0].Storage.ConvertedInstanceVariableExpressionAppendix, settings.GenericTValueName, cancellationToken) /
                "; return true;} value = default; return false;}";
        }

        _ = builder *
            "public global::System.Boolean Is(Type type) => ";
#pragma warning disable IDE0045 // Convert to conditional expression
        if(attributes.Count > 1)
        {
            _ = builder.AppendLine("type == __tag switch {")
                .AppendJoin(
                    attributes,
                    (b, a, t) => b.Append(a.CorrespondingTag).Append(" => typeof(").AppendFull(a).AppendLine("),"),
                    cancellationToken) /
                "_ => ".Append(ConstantSources.InvalidTagStateThrow) /
                "};";
        } else
        {
            _ = builder.Append("type == typeof(")
                .AppendFull(attributes[0]) /
                ");";
        }

        _ = builder.Append("public ")
            .Append(settings.GenericTValueName) *
            " As<"
            .Append(settings.GenericTValueName) *
            ">() => ";

        if(attributes.Count > 1)
        {
            _ = builder.AppendLine("__tag switch {")
                .AppendJoin(
                    attributes,
                    (b, a, t) => b.Append(a.CorrespondingTag).AppendLine(" => typeof(")
                    .Append(settings.GenericTValueName) *
                    ") == typeof("
                    .AppendFull(a).Append(")?").Append(a.Storage.ConvertedInstanceVariableExpressionAppendix, settings.GenericTValueName, t)
                    .Append(':').AppendInvalidConversionThrow($"typeof({settings.GenericTValueName})", t).AppendLine(','),
                    cancellationToken) /
                "_ => ".AppendInvalidConversionThrow($"typeof({settings.GenericTValueName})", cancellationToken) /
                "};";
        } else
        {
            _ = builder.Append("typeof(")
                .Append(settings.GenericTValueName) *
                ") == typeof("
                .AppendFull(attributes[0]) /
                ")?".Append(attributes[0].Storage.ConvertedInstanceVariableExpressionAppendix, settings.GenericTValueName, cancellationToken)
                .Append(':').AppendInvalidConversionThrow($"typeof({settings.GenericTValueName})", cancellationToken).AppendLine(';');
        }

        _ = builder.AppendLine("#endregion");
    }
}
