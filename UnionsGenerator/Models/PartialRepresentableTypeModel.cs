namespace RhoMicro.CodeAnalysis.UnionsGenerator.Models;

using Microsoft.CodeAnalysis;

using RhoMicro.CodeAnalysis.UnionsGenerator.Transformation.Visitors;
using RhoMicro.CodeAnalysis.UnionsGenerator.Utils;

using System.Collections.Immutable;
using System.Threading;

/// <summary>
/// Models a partial model of a union types representable type.
/// </summary>
/// <param name="Alias"></param>
/// <param name="FactoryName"></param>
/// <param name="Options"></param>
/// <param name="Storage"></param>
/// <param name="Groups"></param>
/// <param name="Signature"></param>
/// <param name="OmitConversionOperators"></param>
/// <param name="IsBaseClassToUnionType"></param>
record PartialRepresentableTypeModel(
    String Alias,
    String FactoryName,
    UnionTypeOptions Options,
    StorageOption Storage,
    EquatableList<String> Groups,
    TypeSignatureModel Signature,
    Boolean OmitConversionOperators,
    Boolean IsBaseClassToUnionType) :
    IModel<PartialRepresentableTypeModel>
{
    public static PartialRepresentableTypeModel Create(
        String? alias,
        String factoryName,
        UnionTypeOptions options,
        StorageOption storage,
        EquatableList<String> groups,
        TypeOrTypeParameterType representableType,
        INamedTypeSymbol unionType,
        CancellationToken cancellationToken)
    {
        Throw.ArgumentNull(groups, nameof(groups));

        cancellationToken.ThrowIfCancellationRequested();
        var isNullableAnnotated = options.HasFlag(UnionTypeOptions.Nullable);
        var typeModel = TypeSignatureModel.Create(representableType.UnifiedType, isNullableAnnotated, cancellationToken);

        var nonNullAlias = alias ?? typeModel.Names.IdentifierOrHintName;

        var omitConversionOperators = IsOperatorConversionsOmitted(representableType, unionType, out var isBaseClassToUnionType, cancellationToken);

        var result = new PartialRepresentableTypeModel(
            nonNullAlias,
            factoryName,
            options,
            storage,
            groups,
            typeModel,
            omitConversionOperators,
            isBaseClassToUnionType);

        return result;
    }
    private static Boolean IsOperatorConversionsOmitted(TypeOrTypeParameterType representableType, INamedTypeSymbol unionType, out Boolean unionTypeInheritsRepresentableType, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        unionTypeInheritsRepresentableType = InheritsFrom(unionType, representableType.UnifiedType, cancellationToken);
        if(unionTypeInheritsRepresentableType)
            return true;

        //cancellationToken.ThrowIfCancellationRequested();
        //if(representableType.IsTypeParameter)
        //    return true;

        cancellationToken.ThrowIfCancellationRequested();
        if(representableType.UnifiedType.TypeKind == TypeKind.Interface)
            return true;

        return false;
    }
    private static Boolean InheritsFrom(ITypeSymbol subtype, ITypeSymbol supertype, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var baseTypes = getBaseTypes(subtype);
        if(baseTypes.Contains(supertype, SymbolEqualityComparer.Default))
            return true;

        var interfaces = subtype.AllInterfaces;
        return interfaces.Contains(supertype, SymbolEqualityComparer.Default);

        IEnumerable<INamedTypeSymbol> getBaseTypes(ITypeSymbol symbol)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var baseType = symbol.BaseType;
            while(baseType != null)
            {
                cancellationToken.ThrowIfCancellationRequested();

                yield return baseType;

                baseType = baseType.BaseType;
            }
        }
    }
    /// <inheritdoc/>
    public virtual void Receive<TVisitor>(TVisitor visitor)
        where TVisitor : IVisitor<PartialRepresentableTypeModel>
        => visitor.Visit(this);
}
