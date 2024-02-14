namespace RhoMicro.CodeAnalysis.UnionsGenerator.Models;

using Microsoft.CodeAnalysis;

using RhoMicro.CodeAnalysis.UnionsGenerator.Transformation.Visitors;
using RhoMicro.CodeAnalysis.UnionsGenerator.Utils;

using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

internal record PartialUnionTypeModel(
    TypeSignatureModel Signature,
    PartialRepresentableTypeModel RepresentableType,
    Boolean IsEqualsRequired)
    : IModel<PartialUnionTypeModel>
{
    public static PartialUnionTypeModel? CreateFromTypeParameter(ITypeParameterSymbol target, CancellationToken cancellationToken) =>
        CreateFromTypeParameter(
            target,
            target.GetAttributes()
                .Where(Qualifications.IsUnionTypeParameterAttribute)
                .ToImmutableArray(),
            cancellationToken);
    public static PartialUnionTypeModel? CreateFromTypeParameter(GeneratorAttributeSyntaxContext context, CancellationToken cancellationToken)
    {
        //transform parent declaration of type parameter target of [UnionType]

        cancellationToken.ThrowIfCancellationRequested();
        //check that target is type param
        if(context.TargetSymbol is not ITypeParameterSymbol typeParam)
            return null;

        var result = CreateFromTypeParameter(typeParam, context.Attributes, cancellationToken);

        return result;
    }
    private static PartialUnionTypeModel? CreateFromTypeParameter(ITypeParameterSymbol target, ImmutableArray<AttributeData> attributes, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if(attributes.Length < 1)
            return null;

        cancellationToken.ThrowIfCancellationRequested();
        if(!AliasedUnionTypeBaseAttribute.TryCreate(attributes[0], out var attribute))
            return null;

        cancellationToken.ThrowIfCancellationRequested();
        var signature = TypeSignatureModel.Create(target.ContainingType, cancellationToken);
        var representableType = attribute!.GetPartialModel(new(target), target.ContainingType, cancellationToken);
        var containingType = target.ContainingType;
        var isEqualsRequired = IsEqualsRequiredForTarget(containingType);

        var result = new PartialUnionTypeModel(signature, representableType, isEqualsRequired);

        return result;
    }

    private static Boolean IsEqualsRequiredForTarget(INamedTypeSymbol target) =>
        !target
        .GetMembers("Equals")
        .OfType<IMethodSymbol>()
        .Any(m => m.Parameters.Length == 1 &&
            SymbolEqualityComparer.IncludeNullability.Equals(
                target.WithNullableAnnotation(NullableAnnotation.Annotated),
                m.Parameters[0].Type.WithNullableAnnotation(NullableAnnotation.Annotated)));

    public static EquatableList<PartialUnionTypeModel> CreateFromTypeDeclaration(
        INamedTypeSymbol target,
        CancellationToken cancellationToken) =>
        CreateFromTypeDeclaration(
            target,
            target.GetAttributes()
                .Where(Qualifications.IsUnionTypeDeclarationAttribute)
                .ToImmutableArray(),
            cancellationToken);
    public static EquatableList<PartialUnionTypeModel> CreateFromTypeDeclaration(GeneratorAttributeSyntaxContext context, CancellationToken cancellationToken)
    {
        //transform target of [UnionType<T>]

        cancellationToken.ThrowIfCancellationRequested();
        //check that target of [UnionType<T>] is regular struct or class
        if(context.TargetSymbol is not INamedTypeSymbol target)
            return EquatableList<PartialUnionTypeModel>.Empty;

        var result = CreateFromTypeDeclaration(target, context.Attributes, cancellationToken);

        return result;
    }
    private static EquatableList<PartialUnionTypeModel> CreateFromTypeDeclaration(
        INamedTypeSymbol target,
        ImmutableArray<AttributeData> attributes,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if(target.TypeKind is not TypeKind.Struct and not TypeKind.Class ||
           target.IsRecord)
        {
            return EquatableList<PartialUnionTypeModel>.Empty;
        }

        var result = attributes
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

        var signature = TypeSignatureModel.Create(target, cancellationToken);
        var isEqualsRequired = IsEqualsRequiredForTarget(target);

        for(var i = 0; i < args.Length; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            //check that T in [UnionType<T>] is valid representable type
            if(args[i] is INamedTypeSymbol named)
            {
                var representableType = attribute!.GetPartialModel(new(named), target, cancellationToken);

                yield return new(signature, representableType, isEqualsRequired);
            }
        }
    }
    public void Receive<TVisitor>(TVisitor visitor)
        where TVisitor : IVisitor<PartialUnionTypeModel>
        => visitor.Visit(this);
}
