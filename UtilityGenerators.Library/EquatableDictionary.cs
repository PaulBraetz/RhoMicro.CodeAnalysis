namespace RhoMicro.CodeAnalysis.Library;

using System.Collections;

sealed class EquatableDictionary<TKey, TValue>(IReadOnlyDictionary<TKey, TValue> decorated) :
    IReadOnlyDictionary<TKey, TValue>, IEquatable<EquatableDictionary<TKey, TValue>?>
{
    private readonly IReadOnlyDictionary<TKey, TValue> _decorated = decorated;
    private readonly EqualityComparer<TValue> _valueComparer = EqualityComparer<TValue>.Default;
    private readonly EqualityComparer<TKey> _keyComparer = EqualityComparer<TKey>.Default;

    public Boolean ContainsKey(TKey key) => _decorated.ContainsKey(key);
    public Boolean TryGetValue(TKey key, out TValue value) => _decorated.TryGetValue(key, out value);

    public TValue this[TKey key] => _decorated[key];

    public IEnumerable<TKey> Keys => _decorated.Keys;

    public IEnumerable<TValue> Values => _decorated.Values;

    public Int32 Count => _decorated.Count;

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => _decorated.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_decorated).GetEnumerator();
    public override Boolean Equals(Object? obj) => Equals(obj as EquatableDictionary<TKey, TValue>);
    public Boolean Equals(EquatableDictionary<TKey, TValue>? other)
    {
        if(other is null || Count != other.Count)
            return false;

        foreach(var (key, value) in _decorated)
        {
            if(!other.TryGetValue(key, out var otherValue) ||
               !_valueComparer.Equals(value, otherValue))
            {
                return false;
            }
        }

        return true;
    }

    public override Int32 GetHashCode()
    {
        var sum = 0;

        foreach(var (key, value) in _decorated)
        {
            var hashCode = 594671118;
            hashCode = hashCode * -1521134295 + _keyComparer.GetHashCode(key);
            hashCode = hashCode * -1521134295 + _valueComparer.GetHashCode(value);

            sum += hashCode;
        }

        return sum;
    }
}
