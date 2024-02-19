namespace RhoMicro.CodeAnalysis.UnionsGenerator.Models;

using Microsoft.CodeAnalysis;

using RhoMicro.CodeAnalysis.UnionsGenerator.Transformation.Visitors;
using RhoMicro.CodeAnalysis.UnionsGenerator.Utils;

using System;
using System.Collections.Immutable;

internal readonly record struct RelationModel(RelatedTypeModel RelatedType, RelationType RelationType) : IModel<RelationModel>
{
    public static EquatableList<RelationModel> Create(INamedTypeSymbol targetSymbol, CancellationToken cancellationToken) =>
        targetSymbol.GetAttributes()
        .Where(Qualifications.IsRelationAttribute)
        .Where(a => a.AttributeClass != null)
        .SelectMany(a => a.AttributeClass!.TypeArguments)
        .OfType<INamedTypeSymbol>()
        .Select(t => Create(targetSymbol, t, cancellationToken))
        .ToEquatableList(cancellationToken);
    public static RelationModel Create(
        INamedTypeSymbol targetSymbol,
        INamedTypeSymbol relationSymbol,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var relatedType = RelatedTypeModel.Create(relationSymbol, cancellationToken);
        var relationType = GetRelationType(targetSymbol, relationSymbol, relatedType, cancellationToken);

        var result = new RelationModel(relatedType, relationType);

        return result;
    }
    private static RelationType GetRelationType(
        INamedTypeSymbol targetType,
        INamedTypeSymbol relatedType,
        RelatedTypeModel relatedTypeModel,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var isBidirectional = relatedType
            .GetAttributes()
            .Where(a =>
                a.AttributeClass?.MetadataName == RelationAttribute<Object>.MetadataName &&
                a.AttributeClass.TypeArguments.Length == 1 &&
                SymbolEqualityComparer.Default.Equals(
                    a.AttributeClass.TypeArguments[0],
                    targetType))
            .Any();
        if(isBidirectional)
            return RelationType.BidirectionalRelation;

        var relationTypes = relatedTypeModel.RepresentableTypeSignatures
            .Select(s => s.Names.FullGenericNullableName)
            .ToList();
        var targetTypes = RelatedTypeModel.Create(targetType, cancellationToken).RepresentableTypeSignatures
            .Select(s => s.Names.FullGenericNullableName)
            .ToList();

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
        return relationTypes.Any(targetTypes.Contains) ? RelationType.Intersection : RelationType.Disjunct;
    }
    public void Receive<TVisitor>(TVisitor visitor)
        where TVisitor : IVisitor<RelationModel>
        => visitor.Visit(this);
}
