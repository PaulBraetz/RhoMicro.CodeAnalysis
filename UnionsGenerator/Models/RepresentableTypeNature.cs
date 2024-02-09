namespace RhoMicro.CodeAnalysis.UnionsGenerator.Models;

using Microsoft.CodeAnalysis;

using RhoMicro.CodeAnalysis;

using System.Collections.Concurrent;

// ATTENTION: order influences type order => changes are breaking
internal enum RepresentableTypeNature
{
    ReferenceType,
    ImpureValueType,
    PureValueType,
    UnknownType
}

internal static class RepresentableTypeNatureFactory
{
    public static RepresentableTypeNature Create(UnionTypeBaseAttribute attribute)
    {
        var representedSymbol = attribute.RepresentableTypeSymbol;

        if(representedSymbol == null)
            return RepresentableTypeNature.UnknownType;

        if(representedSymbol.IsPureValueType())
            return RepresentableTypeNature.PureValueType;

        return representedSymbol.IsValueType
            ? RepresentableTypeNature.ImpureValueType
            : representedSymbol.IsReferenceType ? RepresentableTypeNature.ReferenceType : RepresentableTypeNature.UnknownType;
    }

    private static readonly ConcurrentDictionary<ITypeSymbol, Boolean?> _valueTypeCache = new(SymbolEqualityComparer.Default);

    private static Boolean IsPureValueType(this ITypeSymbol symbol)
    {
        evaluate(symbol);

        if(!_valueTypeCache[symbol].HasValue)
            throw new Exception($"Unable to determine whether {symbol.Name} is value type.");

        var result = _valueTypeCache[symbol]!.Value;

        return result;

        static void evaluate(ITypeSymbol symbol)
        {
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

            if(!symbol.IsValueType)
            {
                _valueTypeCache[symbol] = false;
                return;
            }

            var members = symbol.GetMembers();
            foreach(var member in members)
            {
                if(member is IFieldSymbol field && !field.IsStatic)
                {
                    //is field type uninitialized in cache?
                    if(!_valueTypeCache.ContainsKey(field.Type))
                        //initialize & define
                        evaluate(field.Type);

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
