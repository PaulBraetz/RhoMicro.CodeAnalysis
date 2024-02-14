using RhoMicro.CodeAnalysis;
#pragma warning disable

internal class Program
{
    private static void Main(String[] _0)
    { }
}

[UnionType<DateTime, Double>(Groups = ["ValueType"])]
[UnionType<String>(Alias = "Text")]
[Relation<CongruentUnion, SubsetUnion>]
[Relation<SupersetUnion>]
[Relation<IntersectionUnion>]
readonly partial struct Union;

[UnionType<Double>]
[UnionType<DateTime>]
[UnionType<String>(Alias = "Characters")]
sealed partial class CongruentUnion;

[UnionType<DateTime>]
[UnionType<String>]
partial class SubsetUnion;

[UnionType<DateTime>]
[UnionType<String>]
[UnionType<Double>]
[UnionType<Int32>]
partial struct SupersetUnion;

[UnionType<Int16>]
[UnionType<String>]
[UnionType<Double>]
[UnionType<List<Byte>>]
partial class IntersectionUnion;