namespace RhoMicro.CodeAnalysis.Library.Text;

using System.Threading;
using System;

readonly struct StringOrChar : IEquatable<StringOrChar>, IIndentedStringBuilderAppendable
{
    private readonly Char _charValue;
    private readonly String? _stringValue;
    private readonly Byte _isChar;

    public Boolean IsWhitespace => IsString ?
        String.IsNullOrWhiteSpace(_stringValue!) :
        Char.IsWhiteSpace(_charValue);

    public static StringOrChar Empty { get; } = String.Empty;
    public static StringOrChar NewLine { get; } = '\n';
    public static StringOrChar Comma { get; } = ',';
    public static StringOrChar Semicolon { get; } = ';';
    public static StringOrChar Period { get; } = '.';
    public static StringOrChar Tab { get; } = '\t';
    public static StringOrChar TwoSpaces { get; } = "  ";
    public static StringOrChar FourSpaces { get; } = "    ";
    public static StringOrChar DocCommentSlashes { get; } = "/// ";
    public static StringOrChar CommentSlashes { get; } = "// ";
    public Char this[Int32 index] => IsString ?
        _stringValue![index] :
        index == 0 ?
        _charValue :
        throw new IndexOutOfRangeException();
    public ReadOnlySpan<Char> Slice(Int32 start, Int32 length) => IsString ?
        _stringValue!.AsSpan(start, length) :
        start == 0 && length is 1 or 0 ?
        new ReadOnlySpan<Char>([_charValue]) :
        throw new IndexOutOfRangeException();
    public Int32 Length => IsString ? _stringValue!.Length : 1;
    public Boolean IsString => _isChar == 0;

    private StringOrChar(String stringValue) => _stringValue = stringValue;
    private StringOrChar(Char charValue)
    {
        _charValue = charValue;
        _isChar = 1;
    }

    public static implicit operator StringOrChar(String value) => new(value);
    public static explicit operator String(StringOrChar value) => value.IsString ?
        value._stringValue ?? String.Empty :
        throw new InvalidOperationException();

    public static implicit operator StringOrChar(Char value) => new(value);
    public static explicit operator Char(StringOrChar value) => value.IsString ?
        throw new InvalidOperationException() :
        value._charValue;

    public static Boolean operator ==(StringOrChar left, StringOrChar right) => left.Equals(right);
    public static Boolean operator !=(StringOrChar left, StringOrChar right) => !(left == right);

    public override String ToString() => IsString ?
        _stringValue ?? String.Empty :
        _charValue.ToString();
    public override Boolean Equals(Object? obj) =>
        obj is StringOrChar stringOrChar &&
        Equals(stringOrChar);
    public Boolean Equals(StringOrChar other) =>
        IsString == other.IsString &&
        IsString ?
        (String)this == (String)other :
        (Char)this == (Char)other;
    public override Int32 GetHashCode() =>
        IsString ?
        ((String)this).GetHashCode() :
        ((Char)this).GetHashCode();
    public void AppendTo(IndentedStringBuilder builder)
    {
        _ = IsString ?
        builder.Append(_stringValue ?? String.Empty) :
        builder.Append(_charValue);
    }
}
