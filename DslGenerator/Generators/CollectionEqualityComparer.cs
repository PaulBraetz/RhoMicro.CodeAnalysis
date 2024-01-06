namespace RhoMicro.CodeAnalysis.DslGenerator.Generators;

using System.Collections.Immutable;

sealed class CollectionEqualityComparer<T> : IEqualityComparer<ImmutableArray<T>>
{
    private CollectionEqualityComparer() { }
    public static IEqualityComparer<ImmutableArray<T>> Instance { get; } = new CollectionEqualityComparer<T>();

    private static readonly IEqualityComparer<T> _elementComparer = EqualityComparer<T>.Default;

    public Boolean Equals(ImmutableArray<T> x, ImmutableArray<T> y)
    {
        if(x.IsDefault || y.IsDefault)
            return x.IsDefault == y.IsDefault;
        if(x.Length != y.Length)
            return false;
        for(var i = 0; i < x.Length; i++)
        {
            if(!_elementComparer.Equals(x[i], y[i]))
                return false;
        }

        return true;
    }
    public Int32 GetHashCode(ImmutableArray<T> obj)
    {
        if(obj.IsDefault)
            throw new ArgumentException("Array must be properly initialized in order to calculate hashcode.");

        var hashCode = -1030903623;
        for(var i = 0; i < obj.Length; i++)
        {
            hashCode = hashCode * -1521134295 + _elementComparer.GetHashCode(obj[i]);
        }

        return hashCode;
    }
}
