namespace RhoMicro.CodeAnalysis.UnionsGenerator._Models;

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
            return default;

        cancellationToken.ThrowIfCancellationRequested();
        if(!UnionTypeBaseAttribute.TryCreate(context.Attributes[0], out var attribute))
            return default;

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
            .Select(d => (success: TryCreatePartial(d, target, out var a, cancellationToken), attribute: a))
            .Where(t => t.success)
            .Select(t => t.attribute)
            .ToEquatableList(cancellationToken);

        return result!;
    }
    private static Boolean TryCreatePartial(
        AttributeData attributeData,
        INamedTypeSymbol target,
        [NotNullWhen(true)] out PartialUnionTypeModel? model,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        model = null;

        //check that T in [UnionType<T>] is valid representable type
        if(attributeData.AttributeClass?.TypeArguments.Length != 1 ||
           attributeData.AttributeClass?.TypeArguments[0] is not INamedTypeSymbol named)
        {
            return false;
        }

        cancellationToken.ThrowIfCancellationRequested();
        if(!UnionTypeBaseAttribute.TryCreate(attributeData, out var attribute))
            return false;

        cancellationToken.ThrowIfCancellationRequested();
        var signature = TypeSignatureModel.Create(target, cancellationToken);
        var representableType = attribute!.GetPartialModel(new(named), target, cancellationToken);
        model = new PartialUnionTypeModel(signature, representableType);

        return true;
    }
    public void Receive<TVisitor>(TVisitor visitor)
        where TVisitor : IVisitor<PartialUnionTypeModel>
        => visitor.Visit(this);
}
