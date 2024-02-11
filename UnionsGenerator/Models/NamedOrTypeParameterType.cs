namespace RhoMicro.CodeAnalysis.UnionsGenerator.Models;

using Microsoft.CodeAnalysis;

internal readonly struct TypeOrTypeParameterType : IEquatable<TypeOrTypeParameterType>
{
    private readonly ITypeSymbol? _type;
    private readonly ITypeParameterSymbol? _typeParameter;
    private readonly Byte _tag;
    private const Int32 _namedTag = 1;
    private const Int32 _typeParameterTag = 2;

    public TypeOrTypeParameterType(ITypeSymbol named)
    {
        _type = named;
        _tag = _namedTag;
    }
    public TypeOrTypeParameterType(ITypeParameterSymbol typeParameter)
    {
        _typeParameter = typeParameter;
        _tag = _typeParameterTag;
    }

    public Boolean IsNamed => _tag == _namedTag;
    public Boolean Is(out ITypeSymbol? named)
    {
        var result = _tag == _namedTag;
        named = result ? _type! : default;

        return result;
    }
    public Boolean IsTypeParameter => _tag == _typeParameterTag;
    public Boolean Is(out ITypeParameterSymbol? typeParameter)
    {
        var result = _tag == _typeParameterTag;
        typeParameter = result ? _typeParameter! : default;

        return result;
    }

    public T Match<T>(Func<ITypeSymbol, T> onType, Func<ITypeParameterSymbol, T> onTypeParameter) =>
        _tag switch
        {
            _namedTag => onType(_type!),
            _typeParameterTag => onTypeParameter(_typeParameter!),
            _ => throw new InvalidOperationException("The union type was not initialized correctly.")
        };
    public ITypeSymbol UnifiedType => Match(n => n, p => p);
    public void Switch(Action<ITypeSymbol> onNamed, Action<ITypeParameterSymbol> onTypeParameter)
    {
        switch(_tag)
        {
            case _namedTag:
                onNamed(_type!);
                return;
            case _typeParameterTag:
                onTypeParameter(_typeParameter!);
                return;
            default:
                throw new InvalidOperationException("The union type was not initialized correctly.");
        }
    }

    public override Boolean Equals(Object? obj) =>
        obj is TypeOrTypeParameterType type && Equals(type);
    public Boolean Equals(TypeOrTypeParameterType other) =>
        _tag == other._tag &&
        _tag switch
        {
            _namedTag => SymbolEqualityComparer.Default.Equals(_type, other._type),
            _typeParameterTag => SymbolEqualityComparer.Default.Equals(_typeParameter, other._typeParameter),
            _ => throw new InvalidOperationException("The union type was not initialized correctly.")
        };

    public override Int32 GetHashCode()
    {
        var hashCode = -903911270;
        hashCode = hashCode * -1521134295 + _tag.GetHashCode();
        hashCode = hashCode * -1521134295 + _tag switch
        {
            _namedTag => SymbolEqualityComparer.Default.GetHashCode(_type),
            _typeParameterTag => SymbolEqualityComparer.Default.GetHashCode(_typeParameter),
            _ => throw new InvalidOperationException("The union type was not initialized correctly.")
        };
        ;
        return hashCode;
    }

    public static Boolean operator ==(TypeOrTypeParameterType left, TypeOrTypeParameterType right) => left.Equals(right);
    public static Boolean operator !=(TypeOrTypeParameterType left, TypeOrTypeParameterType right) => !(left == right);
}
