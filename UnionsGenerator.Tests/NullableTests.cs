namespace RhoMicro.CodeAnalysis.UnionsGenerator.Tests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class NullableTests : GeneratorTest
{
    [Fact]
    public void AllowsForNullableValueType()
    {
        TestUnionType(
            """
            using System;
            using RhoMicro.CodeAnalysis;

            [UnionType<Nullable<bool>>]
            partial struct NullableBoolUnion { }
            """,
            s => { });
    }
}
