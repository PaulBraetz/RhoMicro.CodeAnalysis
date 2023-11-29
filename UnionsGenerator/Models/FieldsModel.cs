namespace RhoMicro.CodeAnalysis.UnionsGenerator.Models;

using Microsoft.CodeAnalysis;

using RhoMicro.CodeAnalysis.UnionsGenerator;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

readonly struct FieldsModel
{
    public readonly String SourceText;
    private FieldsModel(String sourceText) => SourceText = sourceText;

    public static IncrementalValuesProvider<SourceCarry<TargetDataModel>>
        Project(IncrementalValuesProvider<SourceCarry<TargetDataModel>> provider)
        => provider.SelectCarry(Create, Integrate);

    private static void Integrate(ModelIntegrationContext<FieldsModel> context) =>
        context.Source.SetFields(context.Model);

    private static FieldsModel Create(ModelCreationContext context)
    {
        var representableTypes = context.TargetData.Annotations.AllRepresentableTypes;

        var host = new StrategySourceHost(context.TargetData);

        representableTypes.ForEach(s => s.Storage.Visit(host));

        var sourceTextBuilder = new StringBuilder();

        host.AppendReferenceTypeContainerField(sourceTextBuilder);

        if(representableTypes.Count > 1)
            _ = sourceTextBuilder.AppendLine("[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]").Append("private readonly Tag __tag;");

        host.AppendValueTypeContainerField(sourceTextBuilder);
        host.AppendDedicatedFields(sourceTextBuilder);

        var sourceText = sourceTextBuilder.ToString();
        var result = new FieldsModel(sourceText);

        return result;
    }
}