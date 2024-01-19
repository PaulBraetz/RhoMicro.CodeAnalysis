namespace RhoMicro.CodeAnalysis.Library;

sealed class CollectionEqualityComparer<T>(IEqualityComparer<T> elementComparer) : IEqualityComparer<IEnumerable<T>>
{
    public static CollectionEqualityComparer<T> Default { get; } = new(EqualityComparer<T>.Default);
    private readonly IEqualityComparer<T> _elementComparer = elementComparer;

    public Boolean Equals(IEnumerable<T> x, IEnumerable<T> y) => x.SequenceEqual(y, _elementComparer);
    public Int32 GetHashCode(IEnumerable<T> obj)
    {
        var hashCode = 997021164;
        foreach(var element in obj)
        {
            hashCode = hashCode * -1521134295 + _elementComparer.GetHashCode(element);
        }

        return hashCode;
    }
}
