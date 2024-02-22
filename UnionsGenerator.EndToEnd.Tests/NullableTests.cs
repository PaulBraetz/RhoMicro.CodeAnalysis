namespace RhoMicro.CodeAnalysis.UnionsGenerator.EndToEnd.Tests;

using System;

public partial class NullableTests
{
    [UnionType<Boolean?>]
    readonly partial struct NullableBoolUnion;

    [UnionType<Int32?>(Alias = "Int", Options = UnionTypeOptions.Nullable)]
    readonly partial struct PedroUnion;
    [UnionType<Int64, Int64?>]
    readonly partial struct PedroLongUnion;

    [Fact]
    public void NullableBoolTrueFactoryCall()
    {
        var u = NullableBoolUnion.Create((Boolean?)true);
        Assert.True(u.IsNullable_of_Boolean);
        Assert.True(u.AsNullable_of_Boolean.HasValue);
        Assert.True(u.AsNullable_of_Boolean.Value);
    }

    [Fact]
    public void NullableBoolFalseFactoryCall()
    {
        var u = NullableBoolUnion.Create((Boolean?)false);
        Assert.True(u.IsNullable_of_Boolean);
        Assert.True(u.AsNullable_of_Boolean.HasValue);
        Assert.False(u.AsNullable_of_Boolean.Value);
    }

    [Fact]
    public void NullableBoolNullFactoryCall()
    {
        var u = NullableBoolUnion.Create((Boolean?)null);
        Assert.True(u.IsNullable_of_Boolean);
        Assert.False(u.AsNullable_of_Boolean.HasValue);
    }
}
