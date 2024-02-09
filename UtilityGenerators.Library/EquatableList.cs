namespace RhoMicro.CodeAnalysis.Library;

using System.Collections;

sealed class EquatableList<T>(IReadOnlyList<T> decorated) : IReadOnlyList<T>, IEquatable<EquatableList<T>?>
{
    public static EquatableList<T> Empty { get; } = new(Array.Empty<T>());

    private readonly IReadOnlyList<T> _decorated = decorated;

    public T this[Int32 index] => _decorated[index];
    public Int32 Count => _decorated.Count;

    public override Boolean Equals(Object? obj) =>
        Equals(obj as EquatableList<T>);
    public Boolean Equals(EquatableList<T>? other) =>
        other is not null &&
        Count == other.Count &&
        _decorated.SequenceEqual(other._decorated);
    public IEnumerator<T> GetEnumerator() => _decorated.GetEnumerator();

    public override Int32 GetHashCode()
    {
        var hashCode = 594671118;
        for(var i = 0; i < _decorated.Count; i++)
        {
            hashCode = hashCode * -1521134295 + EqualityComparer<T>.Default.GetHashCode(_decorated[i]);
        }

        return hashCode;
    }

    IEnumerator IEnumerable.GetEnumerator() => _decorated.GetEnumerator();
}
