namespace RhoMicro.CodeAnalysis.Library;

static partial class EnumerableExtensions
{
    public static IReadOnlyList<T> AsEquatable<T>(this IReadOnlyList<T> list) =>
        list is EquatableList<T> equatable ?
        equatable :
        new(list);
    public static IReadOnlyDictionary<TKey, TValue> AsEquatable<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> list) =>
        list is EquatableDictionary<TKey, TValue> equatable ?
        equatable :
        new(list);
    public static void Deconstruct<TKey, TValue>(this KeyValuePair<TKey, TValue> kvp, out TKey key, out TValue value) =>
        (key, value) = (kvp.Key, kvp.Value);
}