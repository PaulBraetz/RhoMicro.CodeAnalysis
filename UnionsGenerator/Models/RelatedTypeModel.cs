namespace RhoMicro.CodeAnalysis.UnionsGenerator.Models;

using Microsoft.CodeAnalysis;

using RhoMicro.CodeAnalysis.UnionsGenerator._Transformation.Visitors;
using RhoMicro.CodeAnalysis.UnionsGenerator.Utils;

readonly record struct RelatedTypeModel(TypeSignatureModel Signature, EquatableSet<TypeSignatureModel> RepresentableTypes) : IModel<RelatedTypeModel>
{
    public static RelatedTypeModel Create(INamedTypeSymbol relationSymbol, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var representableTypes = GetRepresentableTypes(relationSymbol, cancellationToken);
        var signature = TypeSignatureModel.Create(relationSymbol, cancellationToken);
        var result = new RelatedTypeModel(signature, representableTypes);

        return result;
    }
    private static EquatableSet<TypeSignatureModel> GetRepresentableTypes(INamedTypeSymbol relationSymbol, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var declarationSignatures = relationSymbol.GetAttributes()
            .Where(Qualifications.IsUnionTypeDeclarationAttribute)
            .Select(data =>
            {
                var type = data.AttributeClass!.TypeArguments[0];
                var result = TypeSignatureModel.Create(type, cancellationToken);
                return result;
            });
        var typeParamSignatures = relationSymbol.TypeParameters
            .Select((p, i) => (
                index: i,
                representable: p.GetAttributes()
                    .Where(Qualifications.IsUnionTypeParameterAttribute)
                    .Any()))
            .Where(t => t.representable)
            .Select(t => relationSymbol.TypeArguments[t.index])
            .Select(t => TypeSignatureModel.Create(t, cancellationToken));

        var result = declarationSignatures.Concat(typeParamSignatures)
            .ToEquatableSet(cancellationToken);

        return result;
    }
    public void Receive<TVisitor>(TVisitor visitor)
        where TVisitor : IVisitor<RelatedTypeModel>
        => visitor.Visit(this);
}
