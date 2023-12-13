namespace RhoMicro.CodeAnalysis.UtilityGenerators.Library.Tests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class OperatorDecoratorTests
{

    [Fact]
    public void GeneratesOperators()
    {
        const String expected = "Hello, World!";
        var builder = ExpandingMacroStringBuilder.Create<Int32>().GetOperators(default);
        _ = builder +
            (foo, expected);

        var actual = builder.Decorated.Build();
        Assert.Equal(expected, actual);

        static void foo(ExpandingMacroStringBuilder.OperatorsDecorator<Int32> b, String a) => _ = b + a;
    }
}
