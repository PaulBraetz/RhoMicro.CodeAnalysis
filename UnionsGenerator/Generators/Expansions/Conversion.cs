namespace RhoMicro.CodeAnalysis.UnionsGenerator.Generators.Expansions;

using Microsoft.CodeAnalysis;

using RhoMicro.CodeAnalysis;
using RhoMicro.CodeAnalysis.UtilityGenerators.Library;
using RhoMicro.CodeAnalysis.UnionsGenerator.Generators;
using RhoMicro.CodeAnalysis.UnionsGenerator.Models;

using System.Collections.Immutable;
using System.Linq;
using RhoMicro.CodeAnalysis.UnionsGenerator.Generators.Expansions;

sealed class Conversion(TargetDataModel model) : ExpansionBase(model, Macro.Conversion)
{
    static readonly Action<ExpandingMacroBuilder, RepresentableTypeModel, TargetDataModel> _representableTypeConversion = (
        ExpandingMacroBuilder builder,
        RepresentableTypeModel representableType,
        TargetDataModel data) =>
    {
        var model = data.Symbol;
        var allAttributes = data.Annotations.AllRepresentableTypes;

        if(representableType.Attribute.RepresentableTypeIsGenericParameter &&
           !representableType.Attribute.Options.HasFlag(UnionTypeOptions.SupersetOfParameter))
        {
            return;
        }

        _ = builder /
            (b => Docs.MethodSummary(b,
            b => _ = b * "Converts an instance of " * representableType.DocCommentRef * " to the union type " * data.CommentRef * '.',
            [(Name: "value", Summary: b => _ = b * "The value to convert.")])) *
            "public static implicit operator " * model.ToMinimalOpenString() * '(' * representableType.Names.FullTypeName % " value) => new(value);";

        var generateSolitaryExplicit =
            allAttributes.Count > 1 ||
            !representableType.Attribute.Options.HasFlag(
                UnionTypeOptions.ImplicitConversionIfSolitary);

        if(generateSolitaryExplicit)
        {
            _ = builder *
                (b => Docs.MethodSummary(b,
                summary: b => _ = b * "Converts an instance of " * data.CommentRef * " to the representable type " * representableType.CommentRef * "\"/>.",
                parameters: [(Name: "union", Summary: b => _ = b * "The union to convert.")],
                returns: b => _ = b * "The converted value.")) *
                "public static explicit operator " * representableType.Names.FullTypeName * '(' * model.ToMinimalOpenString() * " union) => ";

            if(allAttributes.Count > 1)
            {
                _ = builder *
                    "union.__tag == " * representableType.GetCorrespondingTag(data) * '?';
            }

            _ = builder * (representableType.Storage.TypesafeInstanceVariableExpression, "union");

            if(allAttributes.Count > 1)
            {
                _ = builder * ':' * (InvalidConversionThrow, $"typeof({representableType.Names.FullTypeName}).Name");
            }

            _ = builder * ';';
        } else
        {
            _ = builder * "public static implicit operator " * representableType.Names.FullTypeName * '(' * model.ToMinimalOpenString() * " union) => " *
                (representableType.Storage.TypesafeInstanceVariableExpression, "union") % ';';
        }
    };
    static readonly Action<ExpandingMacroBuilder, RelationTypeModel, TargetDataModel> _relationConversion = (
        ExpandingMacroBuilder builder,
        RelationTypeModel relation,
        TargetDataModel model) =>
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
        _ = builder *
            "#region " * relationType switch
            {
                RelationType.Congruent => "Congruency with ",
                RelationType.Intersection => "Intersection with ",
                RelationType.Superset => "Superset of ",
                RelationType.Subset => "Subset of ",
                _ => "Relation"
            } * relation.Symbol.Name / "public static " *
            (relationType is RelationType.Congruent or RelationType.Superset ?
            "im" :
            "ex") * "plicit operator " * model.Symbol.Name * '(' * relation.Symbol.ToFullOpenString() /
            " relatedUnion) =>";

        if(targetTypeMap.Count == 1)
        {
            UnknownConversion(
                builder,
                model,
                relationTypeMap.Single().Value,
                targetTypeMap.Single().Value,
                "relatedUnion");
        } else
        {
            TypeSwitchExpression(
                builder,
                targetTypeMap,
                (b) => _ = b.WithOperators(builder.CancellationToken) * (b => UtilFullString(b, b => _ = b * "relatedUnion.RepresentedType")),
                (b, m) => _ = b * m.Value.Names.TypeStringName,
                (b, m) => _ = b.WithOperators(builder.CancellationToken) * (b => UnknownConversion(b, model, relationTypeMap[m.Key], m.Value, "relatedUnion")),
                (b) => _ = b.WithOperators(builder.CancellationToken) % (InvalidConversionThrow, $"typeof({model.Symbol.ToFullOpenString()})"));
        }

        _ = builder % ';';

        //conversion to relation from model
        //public static _plicit operator Relation(Target relatedUnion)
        _ = builder * "public static " *
            (relationType is RelationType.Congruent or RelationType.Subset ?
            "im" :
            "ex") *
            "plicit operator " * relation.Symbol.ToFullOpenString() * '(' * model.Symbol.Name % " union) => ";

        if(relationTypeMap.Count == 1)
        {
            KnownConversion(
                builder,
                relation,
                targetTypeMap.Single().Value,
                relationTypeMap.Single().Value,
                "union");
            _ = builder.AppendLine();
        } else
        {
            SwitchExpression(
                builder,
                relationTypeMap,
                (b) => _ = b * "union.__tag",
                (b, kvp) => _ = b * targetTypeMap[kvp.Key].GetCorrespondingTag(model),
                (b, kvp) => _ = b * (b => KnownConversion(b, relation, targetTypeMap[kvp.Key], kvp.Value, "union")),
                (b) => _ = b % (InvalidConversionThrow, $"typeof({relation.Symbol.ToFullOpenString()})"));
        }

        _ = builder % ';';

        _ = builder.AppendLine("#endregion");
    };

    protected override void Expand(ExpandingMacroBuilder builder)
    {
        var omissions = Model.OperatorOmissions.AllOmissions;
        _ = builder %
            "#region Conversions" %
            (b => b.AppendJoin(
                Model.Annotations.AllRepresentableTypes.Where(a => !omissions.Contains(a)),
                (IExpandingMacroStringBuilder<Macro> b, RepresentableTypeModel a, CancellationToken t) => b.WithOperators(t) * (b => _representableTypeConversion(b, a, Model)),
                b.CancellationToken)) %
            (b => b.AppendJoin(
                Model.Annotations.Relations.Select(r => r.ExtractData(Model)),
                (IExpandingMacroStringBuilder<Macro> b, RelationTypeModel r, CancellationToken t) => b.WithOperators(t) * (b => _relationConversion(b, r, Model)),
                b.CancellationToken)) %
            "#endregion";
    }
}
