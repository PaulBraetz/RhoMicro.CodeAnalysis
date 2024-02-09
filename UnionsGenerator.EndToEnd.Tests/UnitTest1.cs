
namespace UnionsGenerator.EndToEndTests;

using RhoMicro.CodeAnalysis;

public partial class UnitTest1
{
    [UnionType<DateTime>]
    [UnionType<String>]
    [UnionType<Double>]
    [Relation<CongruentUnion>]
    [Relation<SubsetUnion>]
    [Relation<SupersetUnion>]
    [Relation<IntersectionUnion>]
    readonly partial struct Union;

    [UnionType<Double>]
    [UnionType<DateTime>]
    [UnionType<String>]
    sealed partial class CongruentUnion;

    [UnionType<DateTime>]
    [UnionType<String>]
    partial class SubsetUnion;

    [UnionType<DateTime>]
    [UnionType<String>]
    [UnionType<Double>]
    [UnionType<Int32>]
    readonly partial struct SupersetUnion;

    [UnionType<Int16>]
    [UnionType<String>]
    [UnionType<Double>]
    [UnionType<List<Byte>>]
    partial class IntersectionUnion;

    [UnionType<String>(Options = UnionTypeOptions.Nullable)]
    [UnionType<Object>]
    partial class Union<[UnionType()] T>;

    [Fact]
    public void Test1()
    {
    }
}