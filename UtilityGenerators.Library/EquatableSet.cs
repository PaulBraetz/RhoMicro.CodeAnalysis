namespace RhoMicro.CodeAnalysis.Library;

using System.Collections;
using System.Collections.Immutable;

sealed class EquatableSet<T>(IImmutableSet<T> decorated) : IImmutableSet<T>, IEquatable<EquatableSet<T>?>
{
    private readonly IImmutableSet<T> _decorated = decorated;
    public Int32 Count => _decorated.Count;

    public IImmutableSet<T> Add(T value) => _decorated.Add(value);
    public IImmutableSet<T> Clear() => _decorated.Clear();
    public Boolean Contains(T value) => _decorated.Contains(value);
    public override Boolean Equals(Object? obj) =>
        Equals(obj as EquatableSet<T>);
    public Boolean Equals(EquatableSet<T>? other) =>
        other is not null &&
        Count == other.Count &&
        _decorated.All(other._decorated.Contains);
    public IImmutableSet<T> Except(IEnumerable<T> other) => _decorated.Except(other);
    public IEnumerator<T> GetEnumerator() => _decorated.GetEnumerator();

    public override Int32 GetHashCode()
    {
        var hashCode = 594671118;
        foreach(var element in _decorated)
        {
            hashCode = hashCode * -1521134295 + EqualityComparer<T>.Default.GetHashCode(element);
        }

        return hashCode;
    }

    public IImmutableSet<T> Intersect(IEnumerable<T> other) => _decorated.Intersect(other);
    public Boolean IsProperSubsetOf(IEnumerable<T> other) => _decorated.IsProperSubsetOf(other);
    public Boolean IsProperSupersetOf(IEnumerable<T> other) => _decorated.IsProperSupersetOf(other);
    public Boolean IsSubsetOf(IEnumerable<T> other) => _decorated.IsSubsetOf(other);
    public Boolean IsSupersetOf(IEnumerable<T> other) => _decorated.IsSupersetOf(other);
    public Boolean Overlaps(IEnumerable<T> other) => _decorated.Overlaps(other);
    public IImmutableSet<T> Remove(T value) => _decorated.Remove(value);
    public Boolean SetEquals(IEnumerable<T> other) => _decorated.SetEquals(other);
    public IImmutableSet<T> SymmetricExcept(IEnumerable<T> other) => _decorated.SymmetricExcept(other);
    public Boolean TryGetValue(T equalValue, out T actualValue) => _decorated.TryGetValue(equalValue, out actualValue);
    public IImmutableSet<T> Union(IEnumerable<T> other) => _decorated.Union(other);
    IEnumerator IEnumerable.GetEnumerator() => _decorated.GetEnumerator();
}