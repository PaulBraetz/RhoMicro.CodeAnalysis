namespace RhoMicro.CodeAnalysis.DslGenerator.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

#if DSL_GENERATOR
[IncludeFile]
#endif
class Range<T> : IReadOnlyList<T>
{
    public Range(IReadOnlyList<T> values, Int32 lowerBounds, Int32 upperBounds)
    {
        if(lowerBounds < 0)
            throw new ArgumentOutOfRangeException(nameof(lowerBounds), lowerBounds, $"{nameof(lowerBounds)} must be >= 0.");
        if(upperBounds < values.Count - 1)
            throw new ArgumentOutOfRangeException(nameof(upperBounds), upperBounds, $"{nameof(upperBounds)} must be < {nameof(values)}.{nameof(values.Count)}.");
        if(lowerBounds > upperBounds)
            throw new ArgumentOutOfRangeException(nameof(lowerBounds), lowerBounds, $"{nameof(lowerBounds)} must be <= {nameof(upperBounds)}.");

        _values = values;
        LowerBound = lowerBounds;
        UpperBound = upperBounds;
    }
    public Range(IReadOnlyList<T> values, Int32 upperBound)
        : this(values, lowerBounds: 0, upperBound)
    { }
    public Range(IReadOnlyList<T> values)
        : this(values, lowerBounds: 0, upperBounds: values.Count - 1)
    { }

    private readonly IReadOnlyList<T> _values;
    public Int32 LowerBound { get; }
    public Int32 UpperBound { get; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Boolean ContainsIndex(Int32 index) => index >= LowerBound && index <= UpperBound;

    protected virtual ReadOnlySpan<T> GetSlice(Int32 start, Int32 length)
    {
        if(_values is T[] valuesArray)
        {
            return valuesArray.AsSpan(start, length);
        }

        var values = _values.Skip(start).Take(length).ToArray().AsSpan();
        return values;
    }
    public ReadOnlySpan<T> Slice(Int32 start, Int32 length)
    {
        if(!ContainsIndex(start))
            throw new ArgumentOutOfRangeException(nameof(start), start, $"{nameof(start)}({start}) must be in ({nameof(LowerBound)}:{nameof(UpperBound)})=>({LowerBound}:{UpperBound}).");
        if(!ContainsIndex(start + length))
            throw new ArgumentOutOfRangeException(nameof(start), length, $"{nameof(start)}({start}) + {nameof(length)}({length}) = {start + length} must be in ({nameof(LowerBound)}:{nameof(UpperBound)})=>({LowerBound}:{UpperBound}).");

        var slice = GetSlice(start, length);
        return slice;
    }

    public ReadOnlySpan<T> this[System.Range range]
    {
        get
        {
            var (start, length) = range.GetOffsetAndLength(Count);
            var result = Slice(start, length);
            return result;
        }
    }
    public T this[Index index]
    {
        get
        {
            var intIndex = index.GetOffset(Count);
            var result = this[intIndex];

            return result;
        }
    }
    public T this[Int32 index] =>
    ContainsIndex(index) ?
    _values[index] :
            throw new ArgumentOutOfRangeException(nameof(index), index, $"{nameof(index)}({index}) must be in ({nameof(LowerBound)}:{nameof(UpperBound)})=>({LowerBound}:{UpperBound}).");

    public Int32 Count => _values.Count;

    private IEnumerable<T> LimitedConsumables()
    {
        for(var i = LowerBound; i <= UpperBound; i++)
            yield return _values[i];
    }

    public IEnumerator<T> GetEnumerator() => LimitedConsumables().GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => LimitedConsumables().GetEnumerator();

    public static implicit operator Range<T>((IReadOnlyList<T> consumables, Int32 lowerBound, Int32 upperBound) args) =>
        new(args.consumables, args.lowerBound, args.upperBound);
    public static implicit operator Range<T>((IReadOnlyList<T> consumables, Int32 upperBound) args) =>
        new(args.consumables, args.upperBound);
}
