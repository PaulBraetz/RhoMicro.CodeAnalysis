namespace RhoMicro.CodeAnalysis.UnionsGenerator.Models;

using Microsoft.CodeAnalysis;

using RhoMicro.CodeAnalysis.UnionsGenerator._Transformation.Visitors;
using RhoMicro.CodeAnalysis.Library;

using System.Threading;

/// <summary>
/// Represents a types signature.
/// </summary>
/// <param name="ContainingTypes"></param>
/// <param name="TypeArgs"></param>
/// <param name="DeclarationKeyword"></param>
/// <param name="IsTypeParameter"></param>
/// <param name="Nature"></param>
/// <param name="IsNullableAnnotated"></param>
/// <param name="Names"></param>
sealed record TypeSignatureModel(
    EquatableList<TypeSignatureModel> ContainingTypes,
    EquatableList<TypeSignatureModel> TypeArgs,
    String DeclarationKeyword,
    Boolean IsTypeParameter,
    TypeNature Nature,
    Boolean IsNullableAnnotated,
    TypeNamesModel Names)
{
    private Int32 _reifiedState = 0;
    /// <summary>
    /// Creates a new type signature model from a type symbol.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static TypeSignatureModel Create(ITypeSymbol type, CancellationToken cancellationToken)
        => Create(type, isNullableAnnotated: type is { IsReferenceType: true, NullableAnnotation: NullableAnnotation.Annotated }, cancellationToken);
    /// <summary>
    /// Creates a new type signature model from a type symbol.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="isNullableAnnotated"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static TypeSignatureModel Create(ITypeSymbol type, Boolean isNullableAnnotated, CancellationToken cancellationToken)
    {
        var result = Create(type, new(SymbolEqualityComparer.IncludeNullability), isNullableAnnotated, cancellationToken);

        result.Reify();

        return result;
    }
    private static TypeSignatureModel Create(
        ITypeSymbol type,
        Dictionary<ITypeSymbol, TypeSignatureModel> cache,
        Boolean isNullableAnnotated,
        CancellationToken cancellationToken)
    {
        if(cache.TryGetValue(type, out var result))
            return result;

        var declarationKeyword = type switch
        {
            { IsRecord: true, TypeKind: TypeKind.Class } => "record class",
            { IsRecord: true, TypeKind: TypeKind.Struct } => "record struct",
            { TypeKind: TypeKind.Class } => "class",
            { TypeKind: TypeKind.Struct } => "struct",
            { TypeKind: TypeKind.Interface } => "interface",
            _ => String.Empty
        };
        var nature = TypeNatures.Create(new(type), cancellationToken);
        var isTypeParameter = type is ITypeParameterSymbol;
        var containingTypes = new List<TypeSignatureModel>();
        var equatableContainingTypes = containingTypes.AsEquatable();
        var typeArgs = new List<TypeSignatureModel>();
        var equatableTypeArgs = typeArgs.AsEquatable();
        var names = TypeNamesModel.Create(type, equatableContainingTypes, equatableTypeArgs, isNullableAnnotated, cancellationToken);

        result = new(
            equatableContainingTypes,
            equatableTypeArgs,
            declarationKeyword,
            isTypeParameter,
            nature,
            isNullableAnnotated,
            names);

        cache.Add(type, result);

        GetContainingTypeSignatures(type, containingTypes, cache, cancellationToken);

        GetTypeArgs(type, typeArgs, cache, cancellationToken);

        return result;
    }
    private static void GetContainingTypeSignatures(
        ITypeSymbol type,
        List<TypeSignatureModel> signatures,
        Dictionary<ITypeSymbol, TypeSignatureModel> cache,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        appendSignature(type.ContainingType);

        void appendSignature(ITypeSymbol? symbol)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if(symbol == null)
                return;

            appendSignature(symbol.ContainingType);

            var isNullableAnnotated = symbol is { IsReferenceType: true, NullableAnnotation: NullableAnnotation.Annotated };
            var signature = Create(symbol, cache, isNullableAnnotated, cancellationToken);
            signatures.Add(signature);
        }
    }
    private static void GetTypeArgs(
        ITypeSymbol type,
        List<TypeSignatureModel> args,
        Dictionary<ITypeSymbol, TypeSignatureModel> cache,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if(type is not INamedTypeSymbol named)
            return;

        for(var i = 0; i < named.TypeArguments.Length; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var typeArgument = named.TypeArguments[i];
            var isNullableAnnotated = typeArgument is { IsReferenceType: true, NullableAnnotation: NullableAnnotation.Annotated };
            var arg = Create(typeArgument, cache, isNullableAnnotated, cancellationToken);
            args.Add(arg);
        }
    }

    void Reify()
    {
        if(Interlocked.CompareExchange(ref _reifiedState, 1, 0) != 0)
            return;

        Names.Reify();

        for(var i = 0; i < ContainingTypes.Count; i++)
            ContainingTypes[i].Reify();

        for(var i = 0; i < TypeArgs.Count; i++)
            TypeArgs[i].Reify();
    }
    public override String ToString() => $"{Nature} {Names}";
    /// <inheritdoc/>
    public Boolean Equals(TypeSignatureModel? other) =>
        ReferenceEquals(this, other) ||
        other != null &&
        Nature == other.Nature &&
        IsTypeParameter == other.IsTypeParameter &&
        EqualityComparer<TypeNamesModel>.Default.Equals(Names, other.Names);
    /// <inheritdoc/>
    public override Int32 GetHashCode()
    {
        var hashCode = -1817968017;
        hashCode = hashCode * -1521134295 + EqualityComparer<String>.Default.GetHashCode(DeclarationKeyword);
        hashCode = hashCode * -1521134295 + IsTypeParameter.GetHashCode();
        hashCode = hashCode * -1521134295 + Nature.GetHashCode();
        hashCode = hashCode * -1521134295 + EqualityComparer<TypeNamesModel>.Default.GetHashCode(Names);
        return hashCode;
    }
    /// <inheritdoc/>
    public void Receive<TVisitor>(TVisitor visitor)
        where TVisitor : IVisitor<TypeSignatureModel>
        => visitor.Visit(this);
}
