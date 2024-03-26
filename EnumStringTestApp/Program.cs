Console.WriteLine(TestNamespace.FooStrings.Foo); //generated members
Console.WriteLine(TestNamespace.FooStrings.FooBar);
Console.WriteLine(TestNamespace.FooStrings.Bar);
Console.WriteLine(TestNamespace.FooStrings.Baz);

namespace TestNamespace
{
    [RhoMicro.CodeAnalysis.GenerateMemberStringConstants]
    enum Foo: Int64
    {
        Foo,
        FooBar,
        Bar = 23,
        Baz
    }
}