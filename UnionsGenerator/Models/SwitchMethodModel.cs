namespace RhoMicro.CodeAnalysis.UnionsGenerator.Models;
using Microsoft.CodeAnalysis;

using RhoMicro.CodeAnalysis.UnionsGenerator;

using System;
using System.Linq;
using System.Text;

/*
*/

readonly struct SwitchMethodModel
{
    public readonly String SourceText;
    private SwitchMethodModel(String sourceText) => SourceText = sourceText;

    public static IncrementalValuesProvider<SourceCarry<TargetDataModel>>
        Project(IncrementalValuesProvider<SourceCarry<TargetDataModel>> provider)
        => provider.SelectCarry(Create, Integrate);

    static void Integrate(ModelIntegrationContext<SwitchMethodModel> context) =>
        context.Source.SetSwitchMethod(context.Model);
    static SwitchMethodModel Create(ModelCreationContext context)
    {
        var representableTypes = context.TargetData.Annotations.AllRepresentableTypes;
        var target = context.TargetData.Symbol;

        var sourceTextBuilder = representableTypes
            .Select((t, i) => (Type: t, Index: i))
            .Aggregate(
                new StringBuilder("public void Switch("),
                (b, t) => b.Append("global::System.Action<")
                    .AppendFull(t.Type)
                    .Append("> on")
                    .Append(t.Type.Names.SafeAlias)
                    .AppendLine(t.Index == representableTypes.Count - 1 ? String.Empty : ","))
            .Append("){");

        if(representableTypes.Count == 1)
        {
            _ = sourceTextBuilder.Append("on")
                .Append(representableTypes[0].Names.SafeAlias)
                .Append(".Invoke(")
                .Append(representableTypes[0].Storage.GetInstanceVariableExpression())
                .AppendLine(");");
        } else
        {
            _ = sourceTextBuilder.AppendLine("switch(__tag){");
            _ = representableTypes.Aggregate(
                sourceTextBuilder,
                (b, a) => b.Append("case ")
                    .Append(a.CorrespondingTag)
                    .AppendLine(":")
                    .Append("on").Append(a.Names.SafeAlias).Append(".Invoke(")
                    .Append(a.Storage.GetInstanceVariableExpression()).AppendLine(");return;"))
                .AppendLine("default:")
                .Append(ConstantSources.InvalidTagStateThrow).AppendLine(";}");
        }

        var sourceText = sourceTextBuilder.Append('}').ToString();
        var result = new SwitchMethodModel(sourceText);

        return result;
    }
}
