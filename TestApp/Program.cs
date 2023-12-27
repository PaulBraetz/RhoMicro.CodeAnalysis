namespace TestApp;

using RhoMicro.CodeAnalysis;

internal class Program
{
    private static void Main(String[] _0)
    {
        Union u = "Hello, World!";
        var ou = OtherUnion.Create(u);
        Console.WriteLine(ou);
        u = Union.Create(u);
        Console.WriteLine(u);
        ou = OtherUnion.Create(u);
        Console.WriteLine(ou);
        ou = OtherUnion.Create(ou);
        Console.WriteLine(ou);
    }
}

[UnionType(typeof(Int32))]
[UnionType(typeof(String))]
partial class Union;

[UnionType(typeof(String))]
[UnionType(typeof(Int32))]
[UnionType(typeof(Byte))]
partial class OtherUnion;