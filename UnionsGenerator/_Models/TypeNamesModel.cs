namespace RhoMicro.CodeAnalysis.UnionsGenerator._Models;

using System;
using System.Data.SqlTypes;
using System.Security.AccessControl;

using Microsoft.CodeAnalysis;

using RhoMicro.CodeAnalysis.UnionsGenerator._Transformation.Visitors;
using RhoMicro.CodeAnalysis.UnionsGenerator.Utils;

/// <summary>
/// Provides access to various required type name strings.
/// </summary>
/// <param name="containingTypesStringLazy"></param>
/// <param name="fullGenericNameLazy"></param>
/// <param name="fullGenericNullableNameLazy"></param>
/// <param name="fullMetadataNameLazy"></param>
/// <param name="fullOpenGenericNameLazy"></param>
/// <param name="genericNameLazy"></param>
/// <param name="identifierOrHintNameLazy"></param>
/// <param name="fullIdentifierOrHintNameLazy"></param>
/// <param name="openGenericNameLazy"></param>
/// <param name="typeArgsStringLazy"></param>
/// <param name="commentRefStringLazy"></param>
/// <param name="name"></param>
/// <param name="namespace"></param>
sealed class TypeNamesModel(
    Lazy<String> containingTypesStringLazy,
    Lazy<String> fullGenericNameLazy,
    Lazy<String> fullGenericNullableNameLazy,
    Lazy<String> fullMetadataNameLazy,
    Lazy<String> fullOpenGenericNameLazy,
    Lazy<String> genericNameLazy,
    Lazy<String> identifierOrHintNameLazy,
    Lazy<String> fullIdentifierOrHintNameLazy,
    Lazy<String> openGenericNameLazy,
    Lazy<String> typeArgsStringLazy,
    Lazy<String> commentRefStringLazy,
    String name,
    String @namespace) : IModel<TypeNamesModel>, IEquatable<TypeNamesModel?>
{
    /// <summary>
    /// Gets the string required for <c>cref</c> comment use.
    /// </summary>
    public String CommentRefString => commentRefStringLazy.Value;
    /// <summary>
    /// Gets the chain of containing type strings.
    /// </summary>
    public String ContainingTypesString => containingTypesStringLazy.Value;
    /// <summary>
    /// Gets the fully qualified, generic name.
    /// </summary>
    public String FullGenericName => fullGenericNameLazy.Value;
    /// <summary>
    /// Gets the fully qualified, generic name, appending a nullable <c>?</c>, if the type has been declared as a nullable reference type.
    /// </summary>
    public String FullGenericNullableName => fullGenericNullableNameLazy.Value;
    /// <summary>
    /// Gets the fully qualified metadata name.
    /// </summary>
    public String FullMetadataName => fullMetadataNameLazy.Value;
    /// <summary>
    /// Gets the fully qualified, open generic name.
    /// </summary>
    public String FullOpenGenericName => fullOpenGenericNameLazy.Value;
    /// <summary>
    /// Gets the generic name.
    /// </summary>
    public String GenericName => genericNameLazy.Value;
    /// <summary>
    /// Gets a name suitable for identifiers or hints.
    /// </summary>
    public String IdentifierOrHintName => identifierOrHintNameLazy.Value;
    /// <summary>
    /// Gets a fully qualified name suitable for identifiers or hints.
    /// </summary>
    public String FullIdentifierOrHintName => fullIdentifierOrHintNameLazy.Value;
    /// <summary>
    /// Gets the open generic name.
    /// </summary>
    public String OpenGenericName => openGenericNameLazy.Value;
    /// <summary>
    /// Gets a comma-delimited list of generic arguments, enclosed in angled brackets.
    /// </summary>
    public String TypeArgsString => typeArgsStringLazy.Value;

    /// <summary>
    /// Gets the type name.
    /// </summary>
    public String Name { get; } = name;
    /// <summary>
    /// Gets the types full enclosing namespace.
    /// </summary>
    public String Namespace { get; } = @namespace;

    internal void Reify()
    {
        _ = CommentRefString;
        _ = ContainingTypesString;
        _ = FullGenericName;
        _ = FullMetadataName;
        _ = FullOpenGenericName;
        _ = GenericName;
        _ = IdentifierOrHintName;
        _ = FullIdentifierOrHintName;
        _ = OpenGenericName;
        _ = TypeArgsString;
    }

    /// <summary>
    /// Creates a new type names model.
    /// </summary>
    /// <param name="symbol"></param>
    /// <param name="containingTypes"></param>
    /// <param name="typeArgs"></param>
    /// <param name="isNullableAnnotated"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    public static TypeNamesModel Create(
        ITypeSymbol symbol,
        EquatableList<TypeSignatureModel> containingTypes,
        EquatableList<TypeSignatureModel> typeArgs,
        Boolean isNullableAnnotated,
        CancellationToken cancellationToken)
    {
        Throw.ArgumentNull(symbol, nameof(symbol));
        Throw.ArgumentNull(containingTypes, nameof(containingTypes));
        Throw.ArgumentNull(typeArgs, nameof(typeArgs));

        cancellationToken.ThrowIfCancellationRequested();

        var name = symbol.Name;
        var @namespace = symbol.ContainingNamespace.IsGlobalNamespace ?
            String.Empty :
            symbol.ContainingNamespace.ToDisplayString(SymbolDisplayFormats.FullNullable);

        var namespacePeriod = String.IsNullOrEmpty(@namespace) ?
            String.Empty : $"{@namespace}.";

        Lazy<String> containingTypesString, typeArgsString,
            fullMetadataName, fullOpenGenericName,
            genericName, openGenericName,
            fullGenericName, commentRefString,
            identifierOrHintName, fullIdentifierOrHintName,

            containingTypesPeriod, openTypesParamString,

            fullGenericNullableName;

        containingTypesString = new(() => String.Join(".", containingTypes.Select(t => t.Names.GenericName)));
        typeArgsString = new(() => typeArgs.Count > 0 ?
            $"<{String.Join(", ", typeArgs.Select(a => a.IsTypeParameter ? a.Names.Name : a.Names.FullGenericName))}>" :
            String.Empty);

        containingTypesPeriod = new(() => containingTypes.Count != 0 ? $"{containingTypesString.Value}." : String.Empty);

        genericName = new(() => symbol is ITypeParameterSymbol ? symbol.Name : $"{name}{typeArgsString.Value}");
        fullGenericName = new(() => symbol is ITypeParameterSymbol ? symbol.Name : $"{namespacePeriod}{containingTypesPeriod.Value}{genericName.Value}");

        openTypesParamString = new(() => typeArgs.Count != 0 ?
            $"<{String.Concat(Enumerable.Repeat(',', typeArgs.Count - 1))}>" :
            String.Empty);

        openGenericName = new(() => symbol is ITypeParameterSymbol ? String.Empty : $"{name}{openTypesParamString.Value}");
        fullOpenGenericName = new(() => symbol is ITypeParameterSymbol ? String.Empty : $"{namespacePeriod}{containingTypesPeriod.Value}{openGenericName.Value}");

        fullMetadataName = new(() =>
        {
            var typeParamIndex = symbol is ITypeParameterSymbol p ?
                $"!{p.ContainingType.TypeParameters.IndexOf(p, 0, SymbolEqualityComparer.IncludeNullability)}" :
                String.Empty;
            return containingTypes.Count > 0 ?
                        $"{containingTypes[^1].Names.FullMetadataName}{( symbol is ITypeParameterSymbol ? typeParamIndex : $"+{symbol.MetadataName}" )}" :
                        $"{( symbol is ITypeParameterSymbol ? String.Empty : namespacePeriod )}{( symbol is ITypeParameterSymbol ? typeParamIndex : symbol.MetadataName )}";
        });
        identifierOrHintName = new(() => symbol.ToDisplayString(
                SymbolDisplayFormats.FullNullableNoContainingTypesOrNamespaces
                .WithMiscellaneousOptions(SymbolDisplayMiscellaneousOptions.UseAsterisksInMultiDimensionalArrays))
            .Replace(" ", String.Empty)
            .Replace("<", "_of_")
            .Replace(">", String.Empty)
            .Replace("*,", String.Empty)
            .Replace("[", "_array")
            .Replace("]", String.Empty)
            .Replace("*", String.Empty)
            .Replace(",", "_and_"));
        fullIdentifierOrHintName = new(() => $"{namespacePeriod.Replace(".", "_")}{( containingTypes.Count != 0 ? $"{String.Join("_", containingTypes.Select(t => t.Names.IdentifierOrHintName))}_" : String.Empty )}{identifierOrHintName.Value}");

        fullGenericNullableName = isNullableAnnotated
            ? new(() => $"{fullGenericName.Value}?")
            : fullGenericName;

        commentRefString = new(() => fullGenericName.Value.Replace("<", "{").Replace(">", "}"));

        var result = new TypeNamesModel(
            containingTypesStringLazy: containingTypesString,
            fullGenericNameLazy: fullGenericName,
            fullGenericNullableNameLazy: fullGenericNullableName,
            fullMetadataNameLazy: fullMetadataName,
            fullOpenGenericNameLazy: fullOpenGenericName,
            genericNameLazy: genericName,
            identifierOrHintNameLazy: identifierOrHintName,
            fullIdentifierOrHintNameLazy: fullIdentifierOrHintName,
            openGenericNameLazy: openGenericName,
            typeArgsStringLazy: typeArgsString,
            commentRefStringLazy: commentRefString,
            name: name,
            @namespace: @namespace);

        return result;
    }

    /// <inheritdoc/>
    public override Boolean Equals(Object? obj) => Equals(obj as TypeNamesModel);
    /// <inheritdoc/>
    public Boolean Equals(TypeNamesModel? other) =>
        ReferenceEquals(this, other) ||
        other is not null
        && FullGenericName == other.FullGenericNullableName;
    /// <inheritdoc/>
    public override Int32 GetHashCode()
    {
        var hashCode = -43426669;
        hashCode = hashCode * -1521134295 + EqualityComparer<String>.Default.GetHashCode(FullGenericNullableName);
        return hashCode;
    }
    /// <inheritdoc/>
    public void Receive<TVisitor>(TVisitor visitor)
        where TVisitor : IVisitor<TypeNamesModel>
        => visitor.Visit(this);
}
