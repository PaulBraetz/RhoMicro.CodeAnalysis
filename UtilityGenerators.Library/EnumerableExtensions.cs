namespace RhoMicro.CodeAnalysis.Library;

using System.Collections.Immutable;

static partial class EnumerableExtensions
{
    public static EquatableList<T> ToEquatableList<T>(this IEnumerable<T> values, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if(values is EquatableList<T> equatable)
            return equatable;

        if(values is IReadOnlyList<T> list)
            return new(list);

        var result = new List<T>();
        foreach(var value in values)
        {
            cancellationToken.ThrowIfCancellationRequested();
            result.Add(value);
        }

        return new(result);
    }
    public static EquatableSet<T> ToEquatableSet<T>(this IEnumerable<T> values, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if(values is EquatableSet<T> equatable)
            return equatable;

        if(values is IImmutableSet<T> list)
            return new(list);

        var resultBuilder = ImmutableHashSet.CreateBuilder<T>();
        foreach(var value in values)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _ = resultBuilder.Add(value);
        }

        var result = resultBuilder.ToImmutable();

        return new(result);
    }
    public static EquatableSet<T> AsEquatable<T>(this IImmutableSet<T> set) =>
        set is EquatableSet<T> equatable ?
        equatable :
        new(set);
    public static EquatableList<T> AsEquatable<T>(this IReadOnlyList<T> list) =>
        list is EquatableList<T> equatable ?
        equatable :
        new(list);
    public static EquatableDictionary<TKey, TValue> AsEquatable<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> list) =>
        list is EquatableDictionary<TKey, TValue> equatable ?
        equatable :
        new(list);
    public static void Deconstruct<TKey, TValue>(this KeyValuePair<TKey, TValue> kvp, out TKey key, out TValue value) =>
        (key, value) = (kvp.Key, kvp.Value);
}