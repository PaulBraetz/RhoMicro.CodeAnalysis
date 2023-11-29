namespace RhoMicro.CodeAnalysis.UnionsGenerator.Models;

using Microsoft.CodeAnalysis;

using RhoMicro.CodeAnalysis.UnionsGenerator;

using System;
using System.Text;

readonly struct EqualsFunctionsModel
{
    private EqualsFunctionsModel(String sourceText) => SourceText = sourceText;
    public readonly String SourceText;

    public static IncrementalValuesProvider<SourceCarry<TargetDataModel>>
        Project(IncrementalValuesProvider<SourceCarry<TargetDataModel>> provider)
        => provider.SelectCarry(Create, Integrate);

    private static void Integrate(ModelIntegrationContext<EqualsFunctionsModel> context) =>
        context.Source.SetEqualsFunctions(context.Model);

    private static EqualsFunctionsModel Create(ModelCreationContext context)
    {
        var target = context.TargetData.Symbol;
        var attributes = context.TargetData.Annotations;

        var sourceTextBuilder = new StringBuilder("public override Boolean Equals(Object obj) => obj is ")
            .AppendOpen(target).AppendLine(" union && Equals(union);")
            .Append("public Boolean Equals(")
            .AppendOpen(target).AppendLine(" obj) =>");

        if(target.IsReferenceType)
        {
            _ = sourceTextBuilder.Append(" obj != null && ");
        }

        if(attributes.AllRepresentableTypes.Count > 1)
        {
            _ = sourceTextBuilder.AppendLine(" __tag == obj.__tag && __tag switch{")
                .AppendAggregate(
                    attributes.AllRepresentableTypes,
                    (b, t) => b.Append(t.CorrespondingTag)
                        .Append(" => ")
                        .Append(t.Storage.GetEqualsInvocation())
                        .AppendLine(","))
                .Append("_ => ").Append(ConstantSources.InvalidTagStateThrow)
                .Append('}');
        } else if(attributes.AllRepresentableTypes.Count == 1)
        {
            _ = sourceTextBuilder.Append(attributes.AllRepresentableTypes[0].Storage.GetEqualsInvocation());
        }

        var sourceText = sourceTextBuilder.AppendLine(";").ToString();
        var result = new EqualsFunctionsModel(sourceText);

        return result;
    }
}
