namespace RhoMicro.CodeAnalysis.UnionsGenerator._Models;

using Microsoft.CodeAnalysis;

using RhoMicro.CodeAnalysis.UnionsGenerator._Transformation.Visitors;
using RhoMicro.CodeAnalysis.UnionsGenerator.Utils;

using System.Threading;

/// <summary>
/// Models a partial model of a union types representable type.
/// </summary>
/// <param name="Alias"></param>
/// <param name="Options"></param>
/// <param name="Storage"></param>
/// <param name="Groups"></param>
/// <param name="Signature"></param>
/// <param name="OmitConversionOperators"></param>
record PartialRepresentableTypeModel(
    String Alias,
    UnionTypeOptions Options,
    StorageOption Storage,
    EquatableList<String> Groups,
    TypeSignatureModel Signature,
    Boolean OmitConversionOperators) :
    IModel<PartialRepresentableTypeModel>
{
    /// <summary>
    /// Creates a new partial representable type model.
    /// </summary>
    /// <param name="alias"></param>
    /// <param name="options"></param>
    /// <param name="storage"></param>
    /// <param name="groups"></param>
    /// <param name="representableType"></param>
    /// <param name="unionType"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static PartialRepresentableTypeModel Create(
        String? alias,
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

        var omitConversionOperators = IsOperatorConversionsOmitted(representableType, unionType, cancellationToken);

        var result = new PartialRepresentableTypeModel(nonNullAlias, options, storage, groups, typeModel, omitConversionOperators);

        return result;
    }
    private static Boolean IsOperatorConversionsOmitted(TypeOrTypeParameterType representableType, INamedTypeSymbol unionType, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if(representableType.IsTypeParameter)
            return true;

        cancellationToken.ThrowIfCancellationRequested();
        if(InheritsFrom(unionType, representableType.UnifiedType, cancellationToken))
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
