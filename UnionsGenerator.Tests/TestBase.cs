namespace RhoMicro.CodeAnalysis.UnionsGenerator.Tests;

public abstract class TestBase : GeneratorTest
{
    protected TestBase() : base(XunitAssertStrategy.Instance) { }
}
