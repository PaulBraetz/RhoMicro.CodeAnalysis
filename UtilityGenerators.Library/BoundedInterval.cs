namespace RhoMicro.CodeAnalysis.Library;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

/// <summary>
/// Represents a left- and right-bounded interval.
/// </summary>
/// <typeparam name="T">The type of values represented by the interval.</typeparam>
/// <remarks>
/// Initializes a new instance.
/// </remarks>
/// <param name="LeftBound">The intervals left (usually lower) bound.</param>
/// <param name="RightBound">The intervals right (usually upper) bound.</param>
/// <param name="LeftClosed">Indicates whether or not <see cref="LeftBound"/> is to be included in the interval. The default is <see langword="true"/>.</param>
/// <param name="RightClosed">Indicates whether or not <see cref="RightBound"/> is to be included in the interval. The default is <see langword="false"/>.</param>
[DebuggerDisplay("{ToString()}")]
readonly record struct BoundedInterval<T>(T LeftBound, T RightBound, Boolean LeftClosed = true, Boolean RightClosed = false) : IEquatable<BoundedInterval<T>>
{
    /// <summary>
    /// The empty interval.
    /// </summary>
    public static readonly BoundedInterval<T> Empty = new();
    /// <summary>
    /// Returns whether or not a bounded interval is empty, using <see cref="EqualityComparer{T}.Default"/>.
    /// </summary>
    /// <remarks>
    /// A given bounded interval is defined as empty when its left and right bound are equal and it is not left- and right-closed.
    /// </remarks>
    /// <returns><see langword="true"/> if the interval is empty; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Boolean IsEmpty() => IsEmpty(EqualityComparer<T>.Default);
    /// <summary>
    /// Returns whether or not a bounded interval is empty.
    /// </summary>
    /// <remarks>
    /// A given bounded interval is defined as empty when its left and right bound are equal and it is not left- and right-closed.
    /// </remarks>
    /// <param name="comparer">The comparer to use when comparing the left and right bound of the interval.</param>
    /// <returns><see langword="true"/> if the interval is empty; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Boolean IsEmpty(IEqualityComparer<T> comparer)
    {
        var result = comparer.Equals(LeftBound, RightBound) && !(LeftClosed && RightClosed);

        return result;
    }
    /// <summary>
    /// Returns whether or not a bounded interval is degenerate, that is, walking it would yield exactly one element, using <see cref="EqualityComparer{T}.Default"/>.
    /// </summary>
    /// <remarks>
    /// A given bounded interval is defined as degenerate when its left and right bound are equal and it is left- and right-closed.
    /// </remarks>
    /// <returns><see langword="true"/> if the interval is degenerate; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Boolean IsDegenerate()
    {
        var result = IsDegenerate(EqualityComparer<T>.Default);

        return result;
    }
    /// <summary>
    /// Returns whether or not a bounded interval is degenerate, that is, walking it would yield exactly one element.
    /// </summary>
    /// <remarks>
    /// A given bounded interval is defined as degenerate when its left and right bound are equal and it is left- and right-closed.
    /// </remarks>
    /// <param name="comparer">The comparer to use when comparing the left and right bound of the interval.</param>
    /// <returns><see langword="true"/> if the interval is degenerate; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Boolean IsDegenerate(IEqualityComparer<T> comparer)
    {
        var result = comparer.Equals(LeftBound, RightBound) && LeftClosed && RightClosed;

        return result;
    }
    /// <summary>
    /// Walks a bounded interval using a walker function until the intervals right bound has been reached.
    /// </summary>
    /// <param name="walker">The walker, which will be passed the last element and calculate from it the next element.</param>
    /// <param name="comparer">The comparer to be used for determining whether the intervals right bound has been reached.</param>
    /// <returns>An enumerable, enumerating the values yielded by <paramref name="walker"/> while walking the interval.</returns>
    public IEnumerable<T> Walk(Func<T, T> walker, IEqualityComparer<T> comparer)
    {
        if(IsDegenerate(comparer))
        {
            yield return LeftBound;
        } else if(!IsEmpty(comparer))
        {
            var result = LeftBound;
            if(LeftClosed)
                yield return result;

            while(!comparer.Equals(result, RightBound))
            {
                result = walker.Invoke(result);

                yield return result;
            }

            if(RightClosed)
                yield return result;
        }
    }
    /// <summary>
    /// Walks a bounded interval using a walker function until the intervals right bound has been reached, using <see cref="EqualityComparer{T}.Default"/>.
    /// </summary>
    /// <param name="walker">The walker, which will be passed the last element and calculate from it the next element.</param>
    /// <returns>An enumerable, enumerating the values yielded by <paramref name="walker"/> while walking the interval.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IEnumerable<T> Walk(Func<T, T> walker)
    {
        var result = Walk(walker, EqualityComparer<T>.Default);

        return result;
    }
    /// <summary>
    /// Returns whether or not a given left- and right-bounded interval contains a value.
    /// </summary>
    /// <param name="value">The value to check for.</param>
    /// <param name="comparer">The comparer to use for determining the order of values represented by the interval.</param>
    /// <returns><see langword="true"/> if the interval contains <paramref name="value"/>; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Boolean Contains(T value, IComparer<T> comparer)
    {
        var result =
            comparer.Compare(value, RightBound) < (RightClosed ? 1 : 0) &&
            comparer.Compare(value, LeftBound) > (LeftClosed ? -1 : 0);

        return result;
    }
    /// <summary>
    /// Returns whether or not a given left- and right-bounded interval contains a value.
    /// </summary>
    /// <param name="value">The value to check for.</param>
    /// <returns><see langword="true"/> if the interval contains <paramref name="value"/>; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Boolean Contains(T value)
    {
        var result = Contains(value, Comparer<T>.Default);

        return result;
    }
    /// <inheritdoc/>
    public override String ToString()
    {
        var result = $"{bracket(LeftClosed)}{LeftBound}, {RightBound}{bracket(!RightClosed)}";

        return result;

        static Char bracket(Boolean inclusive) => inclusive ? '[' : ']';
    }
    /// <inheritdoc/>
    public Boolean Equals(BoundedInterval<T> other) =>
        IsEmpty() && other.IsEmpty() ||
        IsDegenerate() && other.IsDegenerate() && EqualityComparer<T>.Default.Equals(LeftBound, other.LeftBound) ||
        LeftClosed == other.LeftClosed &&
        RightClosed == other.RightClosed &&
        EqualityComparer<T>.Default.Equals(LeftBound, other.LeftBound) &&
        EqualityComparer<T>.Default.Equals(RightBound, other.RightBound);
    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override Int32 GetHashCode() => (LeftBound, LeftClosed, RightBound, RightClosed).GetHashCode();
}
/// <summary>
/// Contains helper methods for bounded intervals.
/// </summary>
static class BoundedInterval
{
    /// <summary>
    /// Creates a left- and right-bounded interval.
    /// </summary>
    /// <param name="leftBound">The intervals left (usually lower) bound.</param>
    /// <param name="rightBound">The intervals right (usually upper) bound.</param>
    /// <param name="leftClosed">Indicates whether or not <paramref name="leftBound"/> is to be included in the interval. The default is <see langword="true"/>.</param>
    /// <param name="rightClosed">Indicates whether or not <paramref name="rightBound"/> is to be included in the interval. The default is <see langword="false"/>.</param>
    /// <returns>A new left- and right-bounded interval.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static BoundedInterval<T> Create<T>(
        T leftBound,
        T rightBound,
        Boolean leftClosed = true,
        Boolean rightClosed = false)
    {
        var interval = new BoundedInterval<T>(leftBound, rightBound, leftClosed, rightClosed);

        return interval;
    }

    /// <summary>
    /// Creates a single-element interval.
    /// </summary>
    /// <param name="element">The intervals single element.</param>
    /// <returns>The degenerate (left- and right-closed) interval, whose left and right bound are <paramref name="element"/>.</returns>
    public static BoundedInterval<T> CreateDegenerate<T>(T element)
    {
        var result = new BoundedInterval<T>(element, element, true, true);

        return result;
    }
}