namespace RhoMicro.CodeAnalysis.UnionsGenerator.Models;

using Microsoft.CodeAnalysis;

using RhoMicro.CodeAnalysis.UnionsGenerator._Models;

using System;
using System.Collections.Immutable;
using System.Linq;

internal sealed class RelationTypeModel : UnionDataModel
{
    private RelationTypeModel(
        AnnotationDataModel annotations,
        OperatorOmissionModel operatorOmissions,
        RelationType relationType,
        INamedTypeSymbol symbol)
        : base(annotations, operatorOmissions, symbol) =>
        RelationType = relationType;

    public readonly RelationType RelationType;

    //commented out because of breaking rewrite changes
    //public static RelationTypeModel Create(RelationAttribute attribute, TargetDataModel target)
    //{
    //    var (annotations, omissions) = CreateModels(attribute.RelatedTypeSymbol);

    //    var relationType = GetRelationType(annotations, target);

    //    return new RelationTypeModel(annotations, omissions, relationType, attribute.RelatedTypeSymbol);
    //}

    private static RelationType GetRelationType(AnnotationDataModel annotations, TargetDataModel target)
    {

        //commented out because of breaking rewrite changes
        var isAlreadyRelated = false;
        //annotations.Relations
        //.Where(a => SymbolEqualityComparer.Default.Equals(a.RelatedTypeSymbol, target.Symbol))
        //.Any();

        if(isAlreadyRelated)
            return RelationType.None;

        var relationTypes = annotations.AllRepresentableTypes.Select(t => t.Names.FullTypeName).ToImmutableHashSet();
        var targetTypes = target.Annotations.AllRepresentableTypes.Select(t => t.Names.FullTypeName).ToImmutableHashSet();

        //is target subset of relation?
        if(targetTypes.All(relationTypes.Contains))
        {
            //is target congruent to relation?
            return relationTypes.Count == targetTypes.Count ?
                 RelationType.Congruent :
                 RelationType.Subset;
        }

        //is target superset of relation?
        if(relationTypes.All(targetTypes.Contains))
            return RelationType.Superset;

        //is relation intersection of target
        return relationTypes.Any(targetTypes.Contains) ? RelationType.Intersection : RelationType.None;
    }
}
