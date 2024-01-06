namespace RhoMicro.CodeAnalysis.UtilityGenerators;
using System.Collections.Immutable;

sealed class ImmutableHashSetCollectionEqualityComparer<T> : IEqualityComparer<ImmutableHashSet<T>>
{
    private ImmutableHashSetCollectionEqualityComparer() { }
    public static ImmutableHashSetCollectionEqualityComparer<T> Instance { get; } = new();

    public Boolean Equals(ImmutableHashSet<T> x, ImmutableHashSet<T> y)
    {
        if(x.Count != y.Count)
        {
            return false;
        }

        if(x.Count == 0)
        {
            return true;
        }

        foreach(var element in x)
        {
            if(!y.Contains(element))
            {
                return false;
            }
        }

        return true;
    }
    public Int32 GetHashCode(ImmutableHashSet<T> obj)
    {
        var hashCode = 997021164;
        foreach(var element in obj)
        {
            hashCode = hashCode * -1521134295 + obj.KeyComparer.GetHashCode(element);
        }

        return hashCode;
    }
}
