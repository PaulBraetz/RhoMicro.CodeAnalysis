namespace RhoMicro.CodeAnalysis.UnionsGenerator.Models;

using System;
using System.Collections.Generic;

internal sealed class KeyValuePairKeyComparer<TKey, TValue> : IEqualityComparer<KeyValuePair<TKey, TValue>>
{
    private KeyValuePairKeyComparer() { }

    public static readonly KeyValuePairKeyComparer<TKey, TValue> Instance = new();
    private static readonly IEqualityComparer<TKey> _keyComparer = EqualityComparer<TKey>.Default;

    public Boolean Equals(KeyValuePair<TKey, TValue> x, KeyValuePair<TKey, TValue> y) => _keyComparer.Equals(x.Key, y.Key);
    public Int32 GetHashCode(KeyValuePair<TKey, TValue> obj) => _keyComparer.GetHashCode(obj.Key);
}
