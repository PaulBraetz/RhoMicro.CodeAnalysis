namespace RhoMicro.CodeAnalysis.UnionsGenerator.Models;
using Microsoft.CodeAnalysis;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

readonly struct DownCastFunctionModel
{
    public readonly String SourceText;
    private DownCastFunctionModel(String sourceText) => SourceText = sourceText;

    public static IncrementalValuesProvider<SourceCarry<TargetDataModel>>
        Project(IncrementalValuesProvider<SourceCarry<TargetDataModel>> provider)
        => provider.SelectCarry(Create, Integrate);

    private static void Integrate(ModelIntegrationContext<DownCastFunctionModel> context) =>
            context.Source.SetDownCastFunction(context.Model);
    private static DownCastFunctionModel Create(ModelCreationContext context)
    {
        var representableTypes = context.TargetData.Annotations.AllRepresentableTypes;
        var target = context.TargetData.Symbol;
        var settings = context.TargetData.Annotations.Settings;

        var sourceTextBuilder = new StringBuilder()
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
            .AppendAggregateJoin(
                ",",
                representableTypes,
                (b, a) => b.AppendFull(a))
            .Append('>');

#pragma warning disable IDE0045 // Convert to conditional expression
        if(representableTypes.Count == 1)
        {
            _ = sourceTextBuilder.Append(" => ")
                .Append(settings.MatchTypeName)
                .Append(".Create<").AppendFull(representableTypes[0])
                .Append(">(").Append(representableTypes[0].Storage.GetInstanceVariableExpression()).Append(')');
        } else
        {
            _ = representableTypes.Aggregate(
                sourceTextBuilder.Append(" => __tag switch{"),
                (b, a) => b.Append(a.CorrespondingTag)
                    .Append(" => ")
                    .Append(settings.MatchTypeName)
                    .Append(".Create<").AppendFull(a)
                    .Append(">(").Append(a.Storage.GetInstanceVariableExpression()).Append(')')
                    .AppendLine(","))
                .Append("_ => ")
                .Append(ConstantSources.InvalidTagStateThrow)
                .Append('}');
        }
#pragma warning restore IDE0045 // Convert to conditional expression

        var sourceText = sourceTextBuilder.Append(';').ToString();
        var result = new DownCastFunctionModel(sourceText);

        return result;
    }
}
