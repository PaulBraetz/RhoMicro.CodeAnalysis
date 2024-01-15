namespace RhoMicro.CodeAnalysis.Library.Text;

using System.Threading;
using System;

readonly struct StringOrChar : IEquatable<StringOrChar>, IIndentedStringBuilderAppendable
{
    private readonly Char _charValue;
    private readonly String? _stringValue;
    private readonly Byte _isString;

    public Boolean IsString => _isString == 1;

    private StringOrChar(String stringValue)
    {
        _stringValue = stringValue;
        _isString = 1;
    }
    private StringOrChar(Char charValue) => _charValue = charValue;

    public static implicit operator StringOrChar(String value) => new(value);
    public static explicit operator String(StringOrChar value) => value.IsString ?
        value._stringValue! :
        throw new InvalidOperationException();

    public static implicit operator StringOrChar(Char value) => new(value);
    public static explicit operator Char(StringOrChar value) => value.IsString ?
        throw new InvalidOperationException() :
        value._charValue;

    public static Boolean operator ==(StringOrChar left, StringOrChar right) => left.Equals(right);
    public static Boolean operator !=(StringOrChar left, StringOrChar right) => !(left == right);

    public override String ToString() => IsString ?
        _stringValue! :
        _charValue.ToString();
    public override Boolean Equals(Object? obj) =>
        obj is StringOrChar stringOrChar &&
        Equals(stringOrChar);
    public Boolean Equals(StringOrChar other) =>
        _charValue == other._charValue &&
        _stringValue == other._stringValue;
    public override Int32 GetHashCode() => (_charValue, _stringValue).GetHashCode();
    public void AppendTo(IndentedStringBuilder builder, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        _ = IsString ?
        builder.Append(_stringValue!) :
        builder.Append(_charValue);
    }
}
