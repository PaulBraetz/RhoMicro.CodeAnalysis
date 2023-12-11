namespace RhoMicro.CodeAnalysis.UnionsGenerator.Generators;

using RhoMicro.CodeAnalysis.UtilityGenerators.Library;
using RhoMicro.CodeAnalysis.UnionsGenerator.Models;

using System.Threading;

sealed class IsAsExpansion(TargetDataModel model) : ExpansionBase(model, Macro.IsAs)
{
    public override void Expand(IExpandingMacroStringBuilder<Macro> builder, CancellationToken cancellationToken)
    {
        var attributes = Model.Annotations.AllRepresentableTypes;
        var target = Model.Symbol;
        var settings = Model.Annotations.Settings;

        _ = builder.AppendLine("#region IsAs")
            .AppendLine("/// <inheritdoc/>")
            .Append("public global::System.Boolean Is<")
            .Append(settings.GenericTValueName)
            .Append(">() => ");
#pragma warning disable IDE0045 // Convert to conditional expression
        if(attributes.Count > 1)
        {
            _ = builder.AppendLine("typeof(")
                .Append(settings.GenericTValueName)
                .Append(") == __tag switch {")
                .AppendJoin(
                    attributes,
                    (b, a, t) => b.Append(a.CorrespondingTag).Append(" => typeof(").AppendFull(a).AppendLine("),"),
                    cancellationToken)
                .AppendLine("_ => ").Append(ConstantSources.InvalidTagStateThrow)
                .AppendLine("};");
        } else
        {
            _ = builder.Append("typeof(")
                .Append(settings.GenericTValueName)
                .Append(") == typeof(")
                .AppendFull(attributes[0])
                .AppendLine(");");
        }

        _ = builder
            .AppendLine("/// <inheritdoc/>")
            .Append("public global::System.Boolean Is(Type type) => ");
#pragma warning disable IDE0045 // Convert to conditional expression
        if(attributes.Count > 1)
        {
            _ = builder.AppendLine("type == __tag switch {")
                .AppendJoin(
                    attributes,
                    (b, a, t) => b.Append(a.CorrespondingTag).Append(" => typeof(").AppendFull(a).AppendLine("),"),
                    cancellationToken)
                .AppendLine("_ => ").Append(ConstantSources.InvalidTagStateThrow)
                .AppendLine("};");
        } else
        {
            _ = builder.Append("type == typeof(")
                .AppendFull(attributes[0])
                .AppendLine(");");
        }

        _ = builder.AppendLine("/// <inheritdoc/>")
            .Append("public ")
            .Append(settings.GenericTValueName)
            .Append(" As<")
            .Append(settings.GenericTValueName)
            .Append(">() => ");

        if(attributes.Count > 1)
        {
            _ = builder.AppendLine("__tag switch {")
                .AppendJoin(
                    attributes,
                    (b, a, t) => b.Append(a.CorrespondingTag).AppendLine(" => typeof(")
                    .Append(settings.GenericTValueName)
                    .Append(") == typeof(")
                    .AppendFull(a).Append(")?").Append(a.Storage.ConvertedInstanceVariableExpressionAppendix, settings.GenericTValueName, t)
                    .Append(':').AppendInvalidConversionThrow($"typeof({settings.GenericTValueName})", t).AppendLine(','),
                    cancellationToken)
                .AppendLine("_ => ").AppendInvalidConversionThrow($"typeof({settings.GenericTValueName})", cancellationToken)
                .AppendLine("};");
        } else
        {
            _ = builder.Append("typeof(")
                .Append(settings.GenericTValueName)
                .Append(") == typeof(")
                .AppendFull(attributes[0])
                .AppendLine(")?").Append(attributes[0].Storage.ConvertedInstanceVariableExpressionAppendix, settings.GenericTValueName, cancellationToken)
                .Append(':').AppendInvalidConversionThrow($"typeof({settings.GenericTValueName})", cancellationToken).AppendLine(';');
        }
#pragma warning restore IDE0045 // Convert to conditional expression
        if(attributes.Count > 1)
        {
            _ = builder.AppendJoin(
                 attributes,
                 (b, a, t) => b.AppendLine("/// <summary>")
                 .Append("/// Gets a value indicating whether this instance is representing a value of type ")
                 .Append(a.DocCommentRef).AppendLine('.')
                 .AppendLine("/// </summary>")
                 .Append("public global::System.Boolean ").Append(a.Names.IsPropertyName)
                 .Append(" => __tag == ").Append(a.CorrespondingTag).AppendLine(';')
                 .AppendLine("/// <summary>")
                 .Append("/// Attempts to retrieve the value represented by this instance as a ")
                 .Append(a.DocCommentRef).AppendLine('.')
                 .AppendLine("/// </summary>")
                 .Append("/// <exception cref=\"global::System.InvalidOperationException\">Thrown if the instance is not representing a value of type ")
                 .Append(a.DocCommentRef).AppendLine(".</exception>")
                 .Append("public ").AppendFull(a)
                 .Append(' ').Append(a.Names.AsPropertyName)
                 .Append(" => __tag == ").Append(a.CorrespondingTag).Append('?')
                 .Append(a.Storage.TypesafeInstanceVariableExpressionAppendix, t)
                 .Append(':')
                 .AppendInvalidConversionThrow($"typeof({a.Names.FullTypeName}).Name", t).AppendLine(';'),
                 cancellationToken);
        } else
        {
            var attribute = attributes[0];

            _ = builder.AppendLine("/// <summary>")
                 .Append("/// Gets a value indicating whether this instance is representing a value of type <c>")
                 .Append(attribute.DocCommentRef).AppendLine("</c>.")
                 .AppendLine("/// </summary>")
                 .Append("public global::System.Boolean Is").Append(attribute.Names.SafeAlias)
                 .AppendLine(" => true;")

                 .AppendLine("/// <summary>")
                 .Append("/// Retrieves the value represented by this instance as a <c>")
                 .Append(attribute.DocCommentRef).AppendLine("</c>.")
                 .AppendLine("/// </summary>")
                 .Append("public ").AppendFull(attribute)
                 .Append(" As").Append(attribute.Names.SafeAlias)
                 .Append(" => ").Append(attribute.Storage.TypesafeInstanceVariableExpressionAppendix, cancellationToken).AppendLine(';');
        }

        _ = builder.AppendLine("#endregion");
    }
}
