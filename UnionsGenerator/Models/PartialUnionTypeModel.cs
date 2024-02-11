namespace RhoMicro.CodeAnalysis.UnionsGenerator.Models;

using Microsoft.CodeAnalysis;

using RhoMicro.CodeAnalysis.UnionsGenerator._Transformation.Visitors;

using System.Diagnostics.CodeAnalysis;
using System.Threading;

internal record PartialUnionTypeModel(
    TypeSignatureModel Signature,
    PartialRepresentableTypeModel RepresentableType)
    : IModel<PartialUnionTypeModel>
{
    public static PartialUnionTypeModel? CreateFromTypeParameter(GeneratorAttributeSyntaxContext context, CancellationToken cancellationToken)
    {
        //transform parent declaration of type parameter target of [UnionType]

        cancellationToken.ThrowIfCancellationRequested();
        //check that target is type param
        if(context.TargetSymbol is not ITypeParameterSymbol typeParam)
            return null;

        cancellationToken.ThrowIfCancellationRequested();
        if(!AliasedUnionTypeBaseAttribute.TryCreate(context.Attributes[0], out var attribute))
            return null;

        cancellationToken.ThrowIfCancellationRequested();
        var signature = TypeSignatureModel.Create(context.TargetSymbol.ContainingType, cancellationToken);
        var representableType = attribute!.GetPartialModel(new(typeParam), typeParam.ContainingType, cancellationToken);
        var result = new PartialUnionTypeModel(signature, representableType);

        return result;
    }
    public static EquatableList<PartialUnionTypeModel> CreateFromTypeDeclaration(GeneratorAttributeSyntaxContext context, CancellationToken cancellationToken)
    {
        //transform target of [UnionType<T>]

        cancellationToken.ThrowIfCancellationRequested();
        //check that target of [UnionType<T>] is regular struct or class
        if(context.TargetSymbol is not INamedTypeSymbol target ||
           target.TypeKind is not TypeKind.Struct and not TypeKind.Class ||
           target.IsRecord)
        {
            return EquatableList<PartialUnionTypeModel>.Empty;
        }

        var result = context.Attributes
            .SelectMany(data => CreatePartials(data, target, cancellationToken))
            .ToEquatableList(cancellationToken);

        return result!;
    }
    private static IEnumerable<PartialUnionTypeModel> CreatePartials(
        AttributeData attributeData,
        INamedTypeSymbol target,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        cancellationToken.ThrowIfCancellationRequested();
        if(!AliasedUnionTypeBaseAttribute.TryCreate(attributeData, out var attribute))
            yield break;

        cancellationToken.ThrowIfCancellationRequested();
        if(attributeData.AttributeClass?.TypeArguments.Length is 0 or null)
            yield break;

        var args = attributeData.AttributeClass.TypeArguments;
        if(args.Length > 1)
        {
            //ignore invalid aliae for non-alializable attribute usages
            attribute!.Alias = null;
        }

        for(var i = 0; i < args.Length; i++)
        {
            //check that T in [UnionType<T>] is valid representable type
            if(args[i] is INamedTypeSymbol named)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var signature = TypeSignatureModel.Create(target, cancellationToken);
                var representableType = attribute!.GetPartialModel(new(named), target, cancellationToken);
                yield return new(signature, representableType);
            }
        }
    }
    public void Receive<TVisitor>(TVisitor visitor)
        where TVisitor : IVisitor<PartialUnionTypeModel>
        => visitor.Visit(this);
}
