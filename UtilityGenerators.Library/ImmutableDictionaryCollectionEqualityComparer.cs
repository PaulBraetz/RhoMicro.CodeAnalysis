namespace RhoMicro.CodeAnalysis.Library;
using System.Collections.Immutable;

sealed class ImmutableDictionaryCollectionEqualityComparer<TKey, TValue> : IEqualityComparer<ImmutableDictionary<TKey, TValue>>
    where TKey : notnull
{
    private ImmutableDictionaryCollectionEqualityComparer() { }
    public static ImmutableDictionaryCollectionEqualityComparer<TKey, TValue> Instance { get; } = new();

    public Boolean Equals(ImmutableDictionary<TKey, TValue> x, ImmutableDictionary<TKey, TValue> y)
    {
        if(x.Count != y.Count)
        {
            return false;
        }

        if(x.Count == 0)
        {
            return true;
        }

        foreach(var key in x.Keys)
        {
            if(!y.ContainsKey(key))
            {
                return false;
            }
        }

        return true;
    }
    public Int32 GetHashCode(ImmutableDictionary<TKey, TValue> obj)
    {
        var hashCode = 997021164;
        foreach(var element in obj)
        {
            hashCode = hashCode * -1521134295 + obj.KeyComparer.GetHashCode(element.Key);
        }

        return hashCode;
    }
}
