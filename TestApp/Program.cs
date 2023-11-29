namespace TestApp;

using RhoMicro.CodeAnalysis;

using System.Diagnostics;

internal partial class Program
{
    private static void Main(String[] _)
    {
        Console.WriteLine(MyAttribute.MetadataName);
        Console.WriteLine(MyAttribute.SourceText);

        new Dto().CopyTo(new Dto());

        Union u = "Hello, World!";
        Console.WriteLine(u);
    }
}

[UnionType(typeof(String))]
[UnionType(typeof(Int32))]
[UnionType(typeof(Double))]
readonly partial struct Union;

[UnionType(typeof(Int32))]
[UnionType(typeof(Double))]
[Relation(typeof(Union))]
readonly partial struct Subset;

[GenerateCopyTo]
sealed partial class Dto
{
    public String? Value { get; set; }
}

[AttributeUsage(AttributeTargets.All)]
[GenerateFactory]
sealed partial class MyAttribute : Attribute { }