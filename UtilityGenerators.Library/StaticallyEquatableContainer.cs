namespace RhoMicro.CodeAnalysis.Library;

readonly struct StaticallyEquatableContainer<T>(T value, Boolean isEqual) : IEquatable<StaticallyEquatableContainer<T>>
{
    public readonly T Value = value;
    public readonly Boolean IsEqual = isEqual;

    private static readonly Random _rng = new();

    public Boolean Equals(StaticallyEquatableContainer<T> other) => IsEqual;
    public override Boolean Equals(Object obj) => IsEqual;
    public override Int32 GetHashCode() => IsEqual ? 0 : _rng.Next(Int32.MinValue, Int32.MaxValue);
}
