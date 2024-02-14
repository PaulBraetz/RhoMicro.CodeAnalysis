namespace RhoMicro.CodeAnalysis.UnionsGenerator.Models;

using Microsoft.CodeAnalysis;

using RhoMicro.CodeAnalysis.UnionsGenerator.Models.Storage;
using RhoMicro.CodeAnalysis.UnionsGenerator.Transformation.Visitors;
using RhoMicro.CodeAnalysis.UnionsGenerator.Utils;

sealed record RelatedTypeModel(TypeSignatureModel Signature, EquatableSet<TypeSignatureModel> RepresentableTypeSignatures) : IModel<RelatedTypeModel>
{
    public static RelatedTypeModel Create(INamedTypeSymbol relationSymbol, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var representableTypeSignatures = GetRepresentableTypeSignatures(relationSymbol, cancellationToken);
        var signature = TypeSignatureModel.Create(relationSymbol, cancellationToken);
        var result = new RelatedTypeModel(signature, representableTypeSignatures);

        return result;
    }
    private static EquatableSet<TypeSignatureModel> GetRepresentableTypeSignatures(INamedTypeSymbol relationSymbol, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var declarationSignatures = PartialUnionTypeModel.CreateFromTypeDeclaration(relationSymbol, cancellationToken);
        var typeParamSignatures = relationSymbol.TypeParameters
            .Select(p => PartialUnionTypeModel.CreateFromTypeParameter(p, cancellationToken))
            .Where(m => m != null);
        
        var relationTypeSignature = TypeSignatureModel.Create(relationSymbol, cancellationToken);

        var result = declarationSignatures.Concat(typeParamSignatures)
            .Where(m => m!.Signature.Equals(relationTypeSignature))
            .Where(p => p != null)
            .Select(p => p!.RepresentableType.Signature)
            .Distinct()
            .ToEquatableSet(cancellationToken);

        return result;
    }
    public void Receive<TVisitor>(TVisitor visitor)
        where TVisitor : IVisitor<RelatedTypeModel>
        => visitor.Visit(this);
}
