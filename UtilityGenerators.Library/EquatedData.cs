namespace RhoMicro.CodeAnalysis.Library;

readonly struct EquatedData<T>(T value) : IEquatable<EquatedData<T>>
{
    public readonly T Value = value;

    private static readonly Int32 _hashCode = typeof(T).GetHashCode();

    public Boolean Equals(EquatedData<T> other) => true;
    public override Boolean Equals(Object obj) =>
        obj is EquatedData<T> other
        && Equals(other);
    public override Int32 GetHashCode() => _hashCode;

    public static implicit operator T(EquatedData<T> data) => data.Value;
    public static implicit operator EquatedData<T>(T value) => new(value);
}
