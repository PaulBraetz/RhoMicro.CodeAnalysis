namespace RhoMicro.CodeAnalysis.UnionsGenerator.Models;
using Microsoft.CodeAnalysis;

using RhoMicro.CodeAnalysis.UnionsGenerator;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

readonly struct ToStringFunctionModel : IEquatable<ToStringFunctionModel>
{
    private ToStringFunctionModel(String sourceText) => SourceText = sourceText;

    public readonly String SourceText;

    public static IncrementalValuesProvider<SourceCarry<TargetDataModel>>
        Project(IncrementalValuesProvider<SourceCarry<TargetDataModel>> provider)
        => provider.SelectCarry(Create, Integrate);

    static void Integrate(ModelIntegrationContext<ToStringFunctionModel> context) =>
        context.Source.SetToStringFunction(context.Model);

    private static ToStringFunctionModel CreateDetailed(ModelCreationContext context)
    {
        var target = context.TargetData.Symbol;
        var attributes = context.TargetData.Annotations.AllRepresentableTypes;

        var sourceTextBuilder = new StringBuilder()
            .AppendLine("#nullable enable")
            .AppendLine("/// <summary>")
            .AppendLine("/// Returns a string representation of the current instance.")
            .AppendLine("/// </summary>")
            .Append("public override String? ToString(){var stringRepresentation = ");

        AppendSimpleToStringExpression(context, sourceTextBuilder);

        _ = sourceTextBuilder
            .Append("; var result = $\"")
            .Append(target.Name)
            .Append('(');

        _ = attributes.Count == 1 ?
            sourceTextBuilder.Append('<').Append(attributes[0].Names.SimpleTypeName).Append('>') :
            sourceTextBuilder.AppendAggregateJoin(" | ", attributes, (b, a) =>
                    b.Append("{(").Append("__tag == ").Append(a.CorrespondingTag).Append('?')
                    .Append("\"<").Append(a.Names.SafeAlias).Append(">\"").Append(':')
                    .Append('\"').Append(a.Names.SafeAlias).Append("\")}"));

        var sourceText = sourceTextBuilder
            .AppendLine("){{{stringRepresentation}}}\"; return result;}")
            .AppendLine("#nullable restore")
            .ToString();
        var result = new ToStringFunctionModel(sourceText);

        return result;
    }
    private static ToStringFunctionModel CreateSimple(ModelCreationContext context)
    {
        var sourceTextBuilder = new StringBuilder("#nullable enable")
            .AppendLine("/// <summary>")
            .AppendLine("/// Returns a string representation of the current instance.")
            .AppendLine("/// </summary>")
            .Append("public override String? ToString() => ");

        AppendSimpleToStringExpression(context, sourceTextBuilder);

        var sourceText = sourceTextBuilder.AppendLine(";")
            .AppendLine("#nullable restore")
            .ToString();

        var result = new ToStringFunctionModel(sourceText);

        return result;
    }
    private static ToStringFunctionModel CreateOmitted(ModelCreationContext _)
    {
        var sourceText = String.Empty;
        var result = new ToStringFunctionModel(sourceText);

        return result;
    }

    private static void AppendSimpleToStringExpression(ModelCreationContext context, StringBuilder sourceTextBuilder)
    {
        var attributes = context.TargetData.Annotations;

        _ = attributes.AllRepresentableTypes.Count == 1 ?
            sourceTextBuilder.Append(attributes.AllRepresentableTypes[0].Storage.GetToStringInvocation()) :
            sourceTextBuilder.Append("__tag switch{")
                .AppendAggregate(
                    attributes.AllRepresentableTypes,
                    (b, t) => b.Append(t.CorrespondingTag)
                        .Append(" => ").Append(t.Storage.GetToStringInvocation())
                        .AppendLine(","))
                .Append("_ => ").AppendLine(ConstantSources.InvalidTagStateThrow)
                .AppendLine("}");
    }

    public static ToStringFunctionModel Create(ModelCreationContext context)
    {
        var result = context.TargetData.Annotations.Settings.ToStringSetting switch
        {
            ToStringSetting.Simple => CreateSimple(context),
            ToStringSetting.Detailed => CreateDetailed(context),
            _ => CreateOmitted(context)
        };

        return result;
    }

    public override Boolean Equals(Object obj) => obj is ToStringFunctionModel model && Equals(model);
    public Boolean Equals(ToStringFunctionModel other) => SourceText == other.SourceText;
    public override Int32 GetHashCode() => -1893336041 + EqualityComparer<String>.Default.GetHashCode(SourceText);

    public static Boolean operator ==(ToStringFunctionModel left, ToStringFunctionModel right) => left.Equals(right);
    public static Boolean operator !=(ToStringFunctionModel left, ToStringFunctionModel right) => !(left == right);
}
