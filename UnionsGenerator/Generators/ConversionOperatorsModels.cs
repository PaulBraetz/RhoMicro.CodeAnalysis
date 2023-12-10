namespace RhoMicro.CodeAnalysis.UnionsGenerator.Generators;

using Microsoft.CodeAnalysis;

using RhoMicro.CodeAnalysis;
using RhoMicro.CodeAnalysis.Common;
using RhoMicro.CodeAnalysis.UnionsGenerator.Generators;
using RhoMicro.CodeAnalysis.UnionsGenerator.Models;

using System.Collections.Immutable;
using System.Linq;

static class ConversionExpansion
{
    public static IMacroExpansion<Macro> Create(TargetDataModel model)
    {
        var omissions = model.OperatorOmissions.AllOmissions;

        var appendices = model.Annotations.AllRepresentableTypes
            .Where(a => !omissions.Contains(a))
            .Select(attribute => (Appendix<Macro>)((b, t) => AppendRepresentableTypeConversion(b, attribute, model, t)))
            .Concat(model.Annotations.Relations
                .Select(r => r.ExtractData(model))
                .Select(r => (Appendix<Macro>)((b, t) => AppendRelationConversion(b, r, model, t))))
            .ToList();
        var result = MacroExpansion.Create(
            Macro.Conversion,
            (b, t) => b
            .AppendLine("#region Conversions")
            .AppendJoin(
                appendices,
                (b, a, t) => b.Append(a, t),
                t)
            .AppendLine("#endregion"));

        return result;
    }

    static void AppendRepresentableTypeConversion(
        IExpandingMacroStringBuilder<Macro> builder,
        RepresentableTypeModel representableType,
        TargetDataModel data,
        CancellationToken cancellationToken)
    {
        var model = data.Symbol;
        var allAttributes = data.Annotations.AllRepresentableTypes;

        if(representableType.Attribute.RepresentableTypeIsGenericParameter &&
           !representableType.Attribute.Options.HasFlag(UnionTypeOptions.SupersetOfParameter))
        {
            return;
        }

        _ = builder
            .AppendLine("/// <summary>")
            .Append("/// Converts an instance of ").AppendCommentRef(representableType)
            .Append(" to the union type ").AppendCommentRef(data).AppendLine("\"/>.")
            .AppendLine("/// </summary>")
            .AppendLine("/// <param name=\"value\">The value to convert.</param>")
            .AppendLine("/// <returns>The converted value.</returns>")
            .Append("public static implicit operator ")
            .AppendOpen(model)
            .Append('(')
            .AppendFull(representableType)
            .AppendLine(" value) => new(value);");

        var generateSolitaryExplicit =
            allAttributes.Count > 1 ||
            !representableType.Attribute.Options.HasFlag(
                UnionTypeOptions.ImplicitConversionIfSolitary);

        if(generateSolitaryExplicit)
        {
            _ = builder
                .Append("public static explicit operator ")
                .AppendFull(representableType)
                .Append('(')
                .AppendOpen(model)
                .Append(" union) => ");

            if(allAttributes.Count > 1)
            {
                _ = builder
                    .Append("union.__tag == Tag.")
                    .Append(representableType.Names.SafeAlias)
                    .Append('?');
            }

            _ = builder.Append(representableType.Storage.TypesafeInstanceVariableExpressionAppendix, "union", cancellationToken);

            if(allAttributes.Count > 1)
            {
                _ = builder
                    .Append(':')
                    .AppendInvalidConversionThrow($"typeof({representableType.Names.FullTypeName}).Name", cancellationToken);
            }

            _ = builder.Append(';');
        } else
        {
            _ = builder.Append("public static implicit operator ")
                .AppendFull(representableType)
                .Append('(')
                .AppendOpen(model)
                .Append(" union) => ")
                .Append(representableType.Storage.TypesafeInstanceVariableExpressionAppendix, "union", cancellationToken)
                .AppendLine(';');
        }
    }
    static void AppendRelationConversion(
        IExpandingMacroStringBuilder<Macro> builder,
        RelationTypeModel relation,
        TargetDataModel model,
        CancellationToken cancellationToken)
    {
        var relationType = relation.RelationType;

        if(relationType is RelationType.None)
            return;

        var relationTypeSet = relation.Annotations.AllRepresentableTypes
            .Select(t => t.Names.FullTypeName)
            .ToImmutableHashSet();
        //we need two maps because each defines different access to the corresponding AsXX etc. properties
        var targetTypeMap = model.Annotations.AllRepresentableTypes
            .Where(t => relationTypeSet.Contains(t.Names.FullTypeName))
            .ToDictionary(t => t.Names.FullTypeName);
        var relationTypeMap = relation.Annotations.AllRepresentableTypes
            .Where(t => targetTypeMap.ContainsKey(t.Names.FullTypeName))
            .ToDictionary(t => t.Names.FullTypeName);

        //conversion to model from relation
        //public static _plicit operator Target(Relation relatedUnion)
        _ = builder
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
            .Append(model.Symbol.Name)
            .Append('(')
            .AppendFull(relation.Symbol)
            .AppendLine(" relatedUnion) =>");

        _ = (targetTypeMap.Count == 1 ?
            builder.AppendUnknownConversion(
                model,
                relationTypeMap.Single().Value,
                targetTypeMap.Single().Value,
                "relatedUnion") :
            builder.AppendTypeSwitchExpression(
                targetTypeMap,
                (b, t) => b.AppendFullString((b, t) => b.Append("relatedUnion.RepresentedType"), t),
                (b, m, t) => b.Append(m.Value.Names.TypeStringName),
                (b, m, t) => b.AppendUnknownConversion(model, relationTypeMap[m.Key], m.Value, "relatedUnion"),
                (b, t) => b.AppendInvalidConversionThrow($"typeof({model.Symbol.ToFullOpenString()})", t).AppendLine(),
                cancellationToken))
                .AppendLine(';');

        //conversion to relation from model
        //public static _plicit operator Relation(Target relatedUnion)
        _ = builder.Append("public static ")
            .Append(
            relationType is RelationType.Congruent or RelationType.Subset ?
            "im" :
            "ex")
            .Append("plicit operator ")
            .AppendFull(relation.Symbol)
            .Append('(')
            .Append(model.Symbol.Name)
            .AppendLine(" union) => ");

        _ = (relationTypeMap.Count == 1 ?
            builder.AppendKnownConversion(
                relation,
                targetTypeMap.Single().Value,
                relationTypeMap.Single().Value,
                "union",
                cancellationToken).AppendLine() :
            builder.AppendSwitchExpression(
                relationTypeMap,
                (b, t) => b.Append("union.__tag"),
                (b, kvp, t) => b.Append(targetTypeMap[kvp.Key].CorrespondingTag),
                (b, kvp, t) => b.AppendKnownConversion(relation, targetTypeMap[kvp.Key], kvp.Value, "union", t),
                (b, t) => b.AppendInvalidConversionThrow($"typeof({relation.Symbol.ToFullOpenString()})", t).AppendLine(),
                cancellationToken))
            .AppendLine(';');

        _ = builder.AppendLine("#endregion");
    }
}
