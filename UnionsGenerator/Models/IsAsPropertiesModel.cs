namespace RhoMicro.CodeAnalysis.UnionsGenerator.Models;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Formatting;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;

readonly struct IsAsPropertiesModel
{
    public readonly String SourceText;
    private IsAsPropertiesModel(String sourceText) => SourceText = sourceText;

    public static IncrementalValuesProvider<SourceCarry<TargetDataModel>>
        Project(IncrementalValuesProvider<SourceCarry<TargetDataModel>> provider)
        => provider.SelectCarry(Create, Integrate);

    static void Integrate(ModelIntegrationContext<IsAsPropertiesModel> context) =>
        context.Source.SetIsAsProperties(context.Model);

    static IsAsPropertiesModel Create(ModelCreationContext context)
    {
        var attributes = context.TargetData.Annotations.AllRepresentableTypes;
        var target = context.TargetData.Symbol;
        var settings = context.TargetData.Annotations.Settings;

        var sourceTextBuilder = new StringBuilder()
            .AppendLine("/// <inheritdoc/>")
            .Append("public global::System.Boolean Is<")
            .Append(settings.GenericTValueName)
            .Append(">() => ");
#pragma warning disable IDE0045 // Convert to conditional expression
        if(attributes.Count > 1)
        {
            _ = sourceTextBuilder.AppendLine("typeof(")
                .Append(settings.GenericTValueName)
                .Append(") == __tag switch {")
                .AppendAggregate(
                    attributes,
                    (b, a) => b.Append(a.CorrespondingTag).Append(" => typeof(").AppendFull(a).AppendLine("),"))
                .AppendLine("_ => ").Append(ConstantSources.InvalidTagStateThrow)
                .AppendLine("};");
        } else
        {
            _ = sourceTextBuilder.Append("typeof(")
                .Append(settings.GenericTValueName)
                .Append(") == typeof(")
                .AppendFull(attributes[0])
                .AppendLine(");");
        }

        _ = sourceTextBuilder
            .AppendLine("/// <inheritdoc/>")
            .Append("public global::System.Boolean Is(Type type) => ");
#pragma warning disable IDE0045 // Convert to conditional expression
        if(attributes.Count > 1)
        {
            _ = sourceTextBuilder.AppendLine("type == __tag switch {")
                .AppendAggregate(
                    attributes,
                    (b, a) => b.Append(a.CorrespondingTag).Append(" => typeof(").AppendFull(a).AppendLine("),"))
                .AppendLine("_ => ").Append(ConstantSources.InvalidTagStateThrow)
                .AppendLine("};");
        } else
        {
            _ = sourceTextBuilder.Append("type == typeof(")
                .AppendFull(attributes[0])
                .AppendLine(");");
        }

        _ = sourceTextBuilder.AppendLine("/// <inheritdoc/>")
            .Append("public ")
            .Append(settings.GenericTValueName)
            .Append(" As<")
            .Append(settings.GenericTValueName)
            .Append(">() => ");

        if(attributes.Count > 1)
        {
            _ = sourceTextBuilder.AppendLine("__tag switch {")
                .AppendAggregate(
                    attributes,
                    (b, a) => b.Append(a.CorrespondingTag).AppendLine(" => typeof(")
                    .Append(settings.GenericTValueName)
                    .Append(") == typeof(")
                    .AppendFull(a).Append(")?").Append(a.Storage.GetConvertedInstanceVariableExpression(settings.GenericTValueName))
                    .Append(':').Append(ConstantSources.InvalidConversionThrow($"nameof({settings.GenericTValueName})")).AppendLine(","))
                .AppendLine("_ => ").Append(ConstantSources.InvalidConversionThrow($"nameof({settings.GenericTValueName})"))
                .AppendLine("};");
        } else
        {
            _ = sourceTextBuilder.Append("typeof(")
                .Append(settings.GenericTValueName)
                .Append(") == typeof(")
                .AppendFull(attributes[0])
                .AppendLine(")?").Append(attributes[0].Storage.GetConvertedInstanceVariableExpression(settings.GenericTValueName))
                    .Append(':').Append(ConstantSources.InvalidConversionThrow($"nameof({settings.GenericTValueName})")).AppendLine(";");
        }
#pragma warning restore IDE0045 // Convert to conditional expression
        if(attributes.Count > 1)
        {
            _ = sourceTextBuilder.AppendAggregate(
                 attributes,
                 (b, a) => b.AppendLine("/// <summary>")
                 .Append("/// Gets a value indicating whether this instance is representing a value of type ")
                 .Append(a.DocCommentRef).AppendLine(".")
                 .AppendLine("/// </summary>")
                 .Append("public global::System.Boolean ").Append(a.Names.IsPropertyName)
                 .Append(" => __tag == ").Append(a.CorrespondingTag).AppendLine(";")

                 .AppendLine("/// <summary>")
                 .Append("/// Attempts to retrieve the value represented by this instance as a ")
                 .Append(a.DocCommentRef).AppendLine(".")
                 .AppendLine("/// </summary>")
                 .Append("/// <exception cref=\"global::System.InvalidOperationException\">Thrown if the instance is not representing a value of type ")
                 .Append(a.DocCommentRef).AppendLine(".</exception>")
                 .Append("public ").AppendFull(a)
                 .Append(' ').Append(a.Names.AsPropertyName)
                 .Append(" => __tag == ").Append(a.CorrespondingTag).Append('?')
                 .Append(a.Storage.GetInstanceVariableExpression()).Append(':')
                 .Append(ConstantSources.InvalidConversionThrow($"typeof({a.Names.FullTypeName}).Name")).AppendLine(";"));
        } else
        {
            var attribute = attributes[0];

            _ = sourceTextBuilder.AppendLine("/// <summary>")
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
                 .Append(" => ").Append(attribute.Storage.GetInstanceVariableExpression()).AppendLine(";");
        }

        var sourceText = sourceTextBuilder.ToString();
        var result = new IsAsPropertiesModel(sourceText);

        return result;
    }
}
