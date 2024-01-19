namespace RhoMicro.CodeAnalysis.Library;

sealed class EqualityComparerStrategy<T>(Func<T, T, Boolean> equalsStrategy, Func<T, Int32> getHashCodeStrategy) : IEqualityComparer<T>
{
    private readonly Func<T, T, Boolean> _equalsStrategy = equalsStrategy;
    private readonly Func<T, Int32> _getHashCodeStrategy = getHashCodeStrategy;

    public Boolean Equals(T x, T y) => _equalsStrategy.Invoke(x, y);
    public Int32 GetHashCode(T obj) => _getHashCodeStrategy.Invoke(obj);
}
