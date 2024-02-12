namespace RhoMicro.CodeAnalysis.UnionsGenerator.Tests;

public class ConversionTests : TestBase
{
    [Theory]
    [InlineData(
        """
        using RhoMicro.CodeAnalysis;
        [UnionType<System.Collections.IEnumerable>]
        readonly partial struct Union { }
        """)]
    [InlineData(
        """
        using RhoMicro.CodeAnalysis;
        [UnionType<System.IDisposable>]
        readonly partial struct Union { }
        """)]
    [InlineData(
        """
        using RhoMicro.CodeAnalysis;
        interface IInterface { }
        [UnionType<IInterface>]
        readonly partial struct Union { }
        """)]
    public void OmitsInterfaceConversions(String source) =>
        UnionType(source, t => { }, unionTypeName: "Union");
}
