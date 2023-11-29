namespace RhoMicro.CodeAnalysis.UnionsGenerator.Models;

using Microsoft.CodeAnalysis;

using RhoMicro.CodeAnalysis.UnionsGenerator;

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

readonly struct ConversionOperatorsModel
{
    private readonly IEnumerable<ConversionOperatorModel> _models;

    private ConversionOperatorsModel(IEnumerable<ConversionOperatorModel> models) => _models = models;

    public static IncrementalValuesProvider<SourceCarry<TargetDataModel>>
        Project(IncrementalValuesProvider<SourceCarry<TargetDataModel>> provider)
        => provider.SelectCarry(Create, Integrate);

    private static void Integrate(ModelIntegrationContext<ConversionOperatorsModel> context) =>
        context.Source.SetOperators(context.Model._models);

    private static ConversionOperatorsModel Create(ModelCreationContext context)
    {
        var target = context.TargetData;
        var omissions = context.TargetData.OperatorOmissions.AllOmissions;
        var models = target.Annotations.AllRepresentableTypes
            .Where(a => !omissions.Contains(a))
            .Select(attribute => ConversionOperatorModel.Create(attribute, target))
            .Concat(target.Annotations.Relations
                .Select(r => r.ExtractData(context.TargetData))
                .Select(r => ConversionOperatorModel.Create(r, target)))
            .ToArray();

        var result = new ConversionOperatorsModel(models);

        return result;
    }
}

readonly struct ConversionOperatorModel
{
    public readonly String SourceText;

    private ConversionOperatorModel(String sourceText) => SourceText = sourceText;

    public static ConversionOperatorModel Create(
        RepresentableTypeData representableType,
        TargetDataModel data)
    {
        var target = data.Symbol;
        var allAttributes = data.Annotations.AllRepresentableTypes;

        if(representableType.Attribute.RepresentableTypeIsGenericParameter &&
           !representableType.Attribute.Options.HasFlag(UnionTypeOptions.SupersetOfParameter))
        {
            return new(String.Empty);
        }

        var sourceTextBuilder = new StringBuilder()
            .AppendLine("/// <summary>")
            .Append("/// Converts an instance of ").AppendCommentRef(representableType)
            .Append(" to the union type ").AppendCommentRef(data).AppendLine("\"/>.")
            .AppendLine("/// </summary>")
            .AppendLine("/// <param name=\"value\">The value to convert.</param>")
            .AppendLine("/// <returns>The converted value.</returns>")
            .Append("public static implicit operator ")
            .AppendOpen(target)
            .Append('(')
            .AppendFull(representableType)
            .AppendLine(" value) => new(value);");

        var generateSolitaryExplicit =
            allAttributes.Count > 1 ||
            !representableType.Attribute.Options.HasFlag(
                UnionTypeOptions.ImplicitConversionIfSolitary);
        if(generateSolitaryExplicit)
        {
            _ = sourceTextBuilder
                .Append("public static explicit operator ")
                .AppendFull(representableType)
                .Append('(')
                .AppendOpen(target)
                .Append(" union) => ");

            if(allAttributes.Count > 1)
            {
                _ = sourceTextBuilder
                    .Append("union.__tag == Tag.")
                    .Append(representableType.Names.SafeAlias)
                    .Append('?');
            }

            _ = sourceTextBuilder.Append(
                representableType.Storage.GetInstanceVariableExpression("union"));

            if(allAttributes.Count > 1)
            {
                _ = sourceTextBuilder
                    .Append(':')
                    .Append(ConstantSources.InvalidConversionThrow(
                        $"typeof({representableType.Names.FullTypeName}).Name"));
            }

            _ = sourceTextBuilder.Append(';');
        } else
        {
            _ = sourceTextBuilder.Append("public static implicit operator ")
                .AppendFull(representableType)
                .Append('(')
                .AppendOpen(target)
                .Append(" union) => ")
                .Append(representableType.Storage.GetInstanceVariableExpression("union"))
                .AppendLine(";");
        }

        var sourceText = sourceTextBuilder.ToString();
        var result = new ConversionOperatorModel(sourceText);

        return result;
    }
    public static ConversionOperatorModel Create(
        RelationTypeData relation,
        TargetDataModel target)
    {
        var sourceText = CreateRelationConversion(relation, target);
        var result = new ConversionOperatorModel(sourceText);

        return result;
    }

    private static String CreateRelationConversion(RelationTypeData relation, TargetDataModel target)
    {
        var relationType = relation.RelationType;

        if(relationType is RelationType.None)
        {
            return String.Empty;
        }

        var relationTypeSet = relation.Annotations.AllRepresentableTypes
            .Select(t => t.Names.FullTypeName)
            .ToImmutableHashSet();
        //we need two maps because each defines different access to the corresponding AsXX etc. properties
        var targetTypeMap = target.Annotations.AllRepresentableTypes
            .Where(t => relationTypeSet.Contains(t.Names.FullTypeName))
            .ToDictionary(t => t.Names.FullTypeName);
        var relationTypeMap = relation.Annotations.AllRepresentableTypes
            .Where(t => targetTypeMap.ContainsKey(t.Names.FullTypeName))
            .ToDictionary(t => t.Names.FullTypeName);

        //conversion to target from relation
        //public static _plicit operator Target(Relation relatedUnion)
        var sourceTextBuilder = new StringBuilder()
            .Append("#region ")
            .Append(relationType switch
            {
                RelationType.Congruent => "Congruency with ",
                RelationType.Intersection => "Intersection with ",
                RelationType.Superset => "Superset of ",
                RelationType.Subset => "Subset of ",
                _ => "Relation"
            })
            .AppendLine(relation.Symbol.Name)
            .Append("public static ")
            .Append(
            relationType is RelationType.Congruent or RelationType.Superset ?
                "im" :
                "ex")
            .Append("plicit operator ")
            .Append(target.Symbol.Name)
            .Append('(')
            .AppendFull(relation.Symbol)
            .AppendLine(" relatedUnion) =>");

        _ = (targetTypeMap.Count == 1 ?
            sourceTextBuilder.AppendUnknownConversion(
                target,
                relationTypeMap.Single().Value,
                targetTypeMap.Single().Value,
                "relatedUnion") :
            sourceTextBuilder.AppendTypeSwitchExpression(
                targetTypeMap,
                ConstantSources.GetFullString("relatedUnion.RepresentedType"),
                t => t.Value.Names.TypeStringName,
                (b, t) => b.AppendUnknownConversion(target, relationTypeMap[t.Key], t.Value, "relatedUnion"),
                b => b.AppendLine(ConstantSources.InvalidConversionThrow($"typeof({target.Symbol.ToFullString()})"))))
            .AppendLine(";");

        //conversion to relation from target
        //public static _plicit operator Relation(Target relatedUnion)
        _ = sourceTextBuilder.Append("public static ")
            .Append(
            relationType is RelationType.Congruent or RelationType.Subset ?
            "im" :
            "ex")
            .Append("plicit operator ")
            .AppendFull(relation.Symbol)
            .Append('(')
            .Append(target.Symbol.Name)
            .AppendLine(" union) => ");

        _ = (relationTypeMap.Count == 1 ?
            sourceTextBuilder.AppendKnownConversion(
                relation,
                targetTypeMap.Single().Value,
                relationTypeMap.Single().Value,
                "union").AppendLine() :
            sourceTextBuilder.AppendSwitchExpression(
                relationTypeMap,
                b => b.Append("union.__tag"),
                (b, t) => b.Append(targetTypeMap[t.Key].CorrespondingTag),
                (b, t) => b.AppendKnownConversion(relation, targetTypeMap[t.Key], t.Value, "union"),
                b => b.AppendLine(ConstantSources.InvalidConversionThrow($"typeof({relation.Symbol.ToFullString()})"))))
            .AppendLine(";");

        var result = sourceTextBuilder.AppendLine("#endregion").ToString();

        return result;
    }
}
