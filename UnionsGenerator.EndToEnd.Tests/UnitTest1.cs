
namespace UnionsGenerator.EndToEndTests;

using RhoMicro.CodeAnalysis;

public partial class UnitTest1
{
    [UnionType<Double, Int32>(Groups = ["Number"])]
    [UnionType<String>]
    partial class GroupsUnion;

    [UnionType<String, DateTime>]
    [UnionType<Double>]
    [Relation<CongruentUnion>]
    [Relation<SubsetUnion>]
    [Relation<SupersetUnion>]
    [Relation<IntersectionUnion>]
    readonly partial struct Union;

    [UnionType<Double, String, DateTime>]
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
    partial class Union<[UnionType] T>;

    [Fact]
    public void Test1()
    {
    }
}