//TODO: continue here

namespace RhoMicro.CodeAnalysis.UnionsGenerator.Models;

using Microsoft.CodeAnalysis;

using RhoMicro.CodeAnalysis.UnionsGenerator;

using System;
using System.Linq;
using System.Text;

readonly struct ConstructorsModel
{
    public readonly String SourceText;
    private ConstructorsModel(String sourceText) => SourceText = sourceText;

    public static IncrementalValuesProvider<SourceCarry<TargetDataModel>>
        Project(IncrementalValuesProvider<SourceCarry<TargetDataModel>> provider)
        => provider.SelectCarry(Create, Integrate);

    private static void Integrate(ModelIntegrationContext<ConstructorsModel> context) =>
        context.Source.SetConstructors(context.Model);

    private static ConstructorsModel Create(ModelCreationContext context)
    {
        var target = context.TargetData.Symbol;
        var annotations = context.TargetData.Annotations;

        var sourceText = annotations.AllRepresentableTypes
            .Aggregate(new StringBuilder(), (b, a) =>
            {
                var accessibility = context.TargetData.GetSpecificAccessibility(a);

                _ = b.Append(accessibility).Append(' ').Append(target.Name).Append('(')
                    .AppendFull(a).AppendLine(" value){");

                if(annotations.AllRepresentableTypes.Count > 1)
                    _ = b.Append("__tag = ").Append(a.CorrespondingTag).AppendLine(";");

                var assignment = a.Storage.GetInstanceVariableAssignmentExpression("value");
                var result = b.Append(assignment)
                    .AppendLine(";}");

                return result;
            }).ToString();

        var result = new ConstructorsModel(sourceText);

        return result;
    }
}
