namespace RhoMicro.CodeAnalysis.UnionsGenerator._Models;

using Microsoft.CodeAnalysis;

using System.Collections.Concurrent;

/// <summary>
/// Enumerates atypes structural nature.
/// </summary>
// ATTENTION: order influences type order => changes are breaking
public enum TypeNature
{
    /// <summary>
    /// The types nature is unknown.
    /// </summary>
    UnknownType,
    /// <summary>
    /// The type is known to be a reference type.
    /// </summary>
    ReferenceType,
    /// <summary>
    /// The type is known to be a value type that contains reference or impure value types.
    /// </summary>
    ImpureValueType,
    /// <summary>
    /// The type is known to contain only pure value types, and no reference types.
    /// </summary>
    PureValueType
}

internal static class TypeNatures
{
    public static TypeNature Create(TypeOrTypeParameterType type, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if(type.UnifiedType.IsPureValueType(cancellationToken))
            return TypeNature.PureValueType;

        return type.UnifiedType.IsValueType
            ? TypeNature.ImpureValueType
            : type.UnifiedType.IsReferenceType
            ? TypeNature.ReferenceType
            : TypeNature.UnknownType;
    }

    private static readonly ConcurrentDictionary<ITypeSymbol, Boolean?> _valueTypeCache = new(SymbolEqualityComparer.Default);

    private static Boolean IsPureValueType(this ITypeSymbol symbol, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        evaluate(symbol, cancellationToken);

        if(!_valueTypeCache[symbol].HasValue)
            throw new Exception($"Unable to determine whether {symbol.Name} is value type.");

        var result = _valueTypeCache[symbol]!.Value;

        return result;

        static void evaluate(ITypeSymbol symbol, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if(_valueTypeCache.TryGetValue(symbol, out var currentResult))
            {
                //cache could be initialized but undefined (null)
                if(currentResult.HasValue)
                    //cache was not null
                    return;
            } else
            {
                //initialize cache for type
                _valueTypeCache[symbol] = null;
            }

            if(symbol is ITypeParameterSymbol typeParam)
            {
                //The enum constraint guarantees an underlying integral type.
                //Therefore, enum constrained type params will be pure.
                //Otherwise, there is no way to determine purity.
                _valueTypeCache[symbol] =
                    typeParam.ConstraintTypes.Any(t =>
                        t.MetadataName == "Enum" &&
                        t.ContainingNamespace.Name == "System");
                return;
            }

            if(!symbol.IsValueType)
            {
                _valueTypeCache[symbol] = false;
                return;
            }

            var members = symbol.GetMembers();
            foreach(var member in members)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if(member is IFieldSymbol field && !field.IsStatic)
                {
                    //is field type uninitialized in cache?
                    if(!_valueTypeCache.ContainsKey(field.Type))
                        //initialize & define
                        evaluate(field.Type, cancellationToken);

                    var fieldTypeIsValueType = _valueTypeCache[field.Type];
                    if(fieldTypeIsValueType.HasValue && !fieldTypeIsValueType.Value)
                    {
                        //field type was initialized but found not to be value type
                        //apply transitive property
                        _valueTypeCache[symbol] = false;
                        return;
                    }
                }
            }

            //no issues found :)
            _valueTypeCache[symbol] = true;
        }
    }
}