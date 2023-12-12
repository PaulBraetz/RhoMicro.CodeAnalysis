using RhoMicro.CodeAnalysis;

[UnionType(typeof(Int32))]
[UnionType(typeof(String))]
readonly partial struct Union
{
}
[UnionType(typeof(String))]
[Relation(typeof(Union))]
partial class Subset;