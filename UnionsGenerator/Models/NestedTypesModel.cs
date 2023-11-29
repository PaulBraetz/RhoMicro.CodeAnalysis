namespace RhoMicro.CodeAnalysis.UnionsGenerator.Models;

using Microsoft.CodeAnalysis;

using RhoMicro.CodeAnalysis.UnionsGenerator;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
readonly struct NestedTypesModel
{
    public readonly String SourceText;
    private NestedTypesModel(String sourceText) => SourceText = sourceText;

    public static IncrementalValuesProvider<SourceCarry<TargetDataModel>>
        Project(IncrementalValuesProvider<SourceCarry<TargetDataModel>> provider)
        => provider.SelectCarry(Create, Integrate);

    static void Integrate(ModelIntegrationContext<NestedTypesModel> context) =>
            context.Source.SetNestedTypes(context.Model);

    static NestedTypesModel Create(ModelCreationContext context)
    {
        var sourceTextBuilder = new StringBuilder();

        var representableTypes = context.TargetData.Annotations.AllRepresentableTypes;

        if(representableTypes.Count > 1)
        {
            _ = sourceTextBuilder
                .AppendLine("[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]")
                .AppendLine("private enum Tag : Byte {")
                .AppendJoin(',', representableTypes.Select(a => a.Names.SafeAlias))
                .Append('}');
        }

        var sourceText = sourceTextBuilder.ToString();
        var result = new NestedTypesModel(sourceText);

        return result;
    }
}
