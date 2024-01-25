namespace RhoMicro.CodeAnalysis.DslGenerator.Common;
using System;
using System.Collections;
using System.Collections.Generic;

#if DSL_GENERATOR
[IncludeFile]
#endif
sealed class StringRange : Range<Char>
{
    sealed class StringList(String value) : IReadOnlyList<Char>
    {
        public Char this[Int32 index] => value[index];
        public Int32 Count => value.Length;
        public IEnumerator<Char> GetEnumerator() => value.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)value).GetEnumerator();
    }

    public StringRange(String value)
        : base(new StringList(value))
        => _value = value;
    public StringRange(String value, Int32 upperBound)
        : base(new StringList(value), upperBound)
        => _value = value;
    public StringRange(String value, Int32 lowerBounds, Int32 upperBounds)
        : base(new StringList(value), lowerBounds, upperBounds)
        => _value = value;

    private readonly String _value;

    protected override ReadOnlySpan<Char> GetSlice(Int32 start, Int32 length) => _value.AsSpan(start, length);
}
