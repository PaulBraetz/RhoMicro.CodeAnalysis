namespace RhoMicro.CodeAnalysis.Library;
using System.Collections.Immutable;
sealed class ImmutableArrayCollectionEqualityComparer<T>(IEqualityComparer<T> elementComparer) : IEqualityComparer<ImmutableArray<T>>
{
    public static ImmutableArrayCollectionEqualityComparer<T> Default { get; } = new(EqualityComparer<T>.Default);
    private readonly IEqualityComparer<T> _elementComparer = elementComparer;

    public Boolean Equals(ImmutableArray<T> x, ImmutableArray<T> y)
    {
        if(x.Length != y.Length)
        {
            return false;
        }

        if(x.Length == 0)
        {
            return true;
        }

        for(var i = 0; i < x.Length; i++)
        {
            if(!_elementComparer.Equals(x[i], y[i]))
            {
                return false;
            }
        }

        return true;
    }
    public Int32 GetHashCode(ImmutableArray<T> obj)
    {
        var hashCode = 997021164;
        for(var i = 0; i < obj.Length; i++)
        {
            var element = obj[i];
            hashCode = hashCode * -1521134295 + _elementComparer.GetHashCode(element);
        }

        return hashCode;
    }
}
