namespace RhoMicro.CodeAnalysis.UnionsGenerator.Models;

using Microsoft.CodeAnalysis;

using RhoMicro.CodeAnalysis.UnionsGenerator;

using System;
using System.Linq;
using System.Text;
readonly struct GetHashcodeFunctionModel
{
    private GetHashcodeFunctionModel(String sourceText) => SourceText = sourceText;
    public readonly String SourceText;

    public static IncrementalValuesProvider<SourceCarry<TargetDataModel>>
        Project(IncrementalValuesProvider<SourceCarry<TargetDataModel>> provider)
        => provider.SelectCarry(Create, Integrate);

    private static void Integrate(ModelIntegrationContext<GetHashcodeFunctionModel> context) =>
        context.Source.SetGethashcodeFunction(context.Model);

    private static GetHashcodeFunctionModel Create(ModelCreationContext context)
    {
        var annotations = context.TargetData.Annotations;
        var target = context.TargetData.Symbol;

        var sourceTextBuilder = new StringBuilder("public override Int32 GetHashCode() =>");

        if(annotations.AllRepresentableTypes.Count > 1)
        {
            _ = sourceTextBuilder.Append("__tag switch{")
                .AppendAggregate(
                    annotations.AllRepresentableTypes,
                    (b, a) => b.Append(a.CorrespondingTag)
                        .Append(" => ")
                        .Append(a.Storage.GetGetHashCodeInvocation())
                .AppendLine(","))
                .AppendLine("_ => ").Append(ConstantSources.InvalidTagStateThrow)
                .Append('}');

        } else if(annotations.AllRepresentableTypes.Count == 1)
        {
            var storage = annotations.AllRepresentableTypes[0].Storage;
            _ = sourceTextBuilder.Append(storage.GetGetHashCodeInvocation());
        }

        var sourceText = sourceTextBuilder.Append(';').ToString();
        var result = new GetHashcodeFunctionModel(sourceText);

        return result;
    }
}