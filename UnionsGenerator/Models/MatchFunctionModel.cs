namespace RhoMicro.CodeAnalysis.UnionsGenerator.Models;

using Microsoft.CodeAnalysis;

using System;
using System.Linq;
using System.Runtime;
using System.Text;

readonly struct MatchFunctionModel
{
    public readonly String SourceText;
    private MatchFunctionModel(String sourceText) => SourceText = sourceText;

    public static IncrementalValuesProvider<SourceCarry<TargetDataModel>>
        Project(IncrementalValuesProvider<SourceCarry<TargetDataModel>> provider)
        => provider.SelectCarry(Create, Integrate);

    static void Integrate(ModelIntegrationContext<MatchFunctionModel> context) =>
        context.Source.SetMatchFunction(context.Model);
    static MatchFunctionModel Create(ModelCreationContext context)
    {
        var representableType = context.TargetData.Annotations.AllRepresentableTypes;
        var target = context.TargetData.Symbol;
        var settings = context.TargetData.Annotations.Settings;

        var sourceTextBuilder = representableType
            .Select((t, i) => (Type: t, Index: i))
            .Aggregate(
                new StringBuilder("public ").Append(settings.MatchTypeName).Append(" Match<")
                    .Append(settings.MatchTypeName)
                    .Append(">("),
                (b, t) => b.Append("global::System.Func<")
                    .AppendFull(t.Type)
                    .Append(", ")
                    .Append(settings.MatchTypeName)
                    .Append("> on")
                    .Append(t.Type.Names.SafeAlias)
                    .AppendLine(t.Index == representableType.Count - 1 ? String.Empty : ","))
            .AppendLine(") =>");

        if(representableType.Count == 1)
        {
            _ = sourceTextBuilder.Append("on")
                .Append(representableType[0].Names.SafeAlias)
                .Append(".Invoke(")
                .Append(representableType[0].Storage.GetInstanceVariableExpression())
                .AppendLine(");");
        } else
        {
            _ = sourceTextBuilder.AppendLine("__tag switch{");
            _ = representableType.Aggregate(
                sourceTextBuilder,
                (b, a) => b.Append(a.CorrespondingTag)
                    .AppendLine(" => ")
                    .Append("on").Append(a.Names.SafeAlias).Append(".Invoke(")
                    .Append(a.Storage.GetInstanceVariableExpression()).AppendLine("),"))
                .AppendLine("_ =>")
                .AppendLine(ConstantSources.InvalidTagStateThrow)
                .AppendLine("};");
        }

        var sourceText = sourceTextBuilder.ToString();
        var result = new MatchFunctionModel(sourceText);

        return result;
    }
}