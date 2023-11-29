namespace RhoMicro.CodeAnalysis.UnionsGenerator.Models;

using Microsoft.CodeAnalysis;

using RhoMicro.CodeAnalysis.UnionsGenerator;

using System;
using System.Text;

readonly struct ValueTypeContainerModel
{
    public readonly String SourceText;
    private ValueTypeContainerModel(String sourceText) => SourceText = sourceText;

    public static IncrementalValuesProvider<SourceCarry<TargetDataModel>>
        Project(IncrementalValuesProvider<SourceCarry<TargetDataModel>> provider)
        => provider.SelectCarry(Create, Integrate);

    static void Integrate(ModelIntegrationContext<ValueTypeContainerModel> context) =>
            context.Source.SetValueTypeContainer(context.Model);

    static ValueTypeContainerModel Create(ModelCreationContext context)
    {
        var sourceTextBuilder = new StringBuilder();
        var representableTypes = context.TargetData.Annotations.AllRepresentableTypes;

        var host = new StrategySourceHost(context.TargetData);
        representableTypes.ForEach(t => t.Storage.Visit(host));

        host.AppendValueTypeContainerType(sourceTextBuilder);

        var sourceText = sourceTextBuilder.ToString();
        var result = new ValueTypeContainerModel(sourceText);

        return result;
    }
}
