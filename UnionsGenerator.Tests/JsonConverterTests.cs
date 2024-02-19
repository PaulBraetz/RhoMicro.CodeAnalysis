namespace RhoMicro.CodeAnalysis.UnionsGenerator.Tests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;

public class JsonConverterTests() : TestBase(Basic.Reference.Assemblies.Net60.References.All)
{
    [Theory]
    [InlineData(
        """
        using RhoMicro.CodeAnalysis;
        using System;
        
        [UnionType<Int32, String>]
        partial class Union { }
        """)]
    [InlineData(
        """
        using RhoMicro.CodeAnalysis;
        using System;
        
        [UnionType<Int32, String>]
        [UnionTypeSettings(Miscellaneous = MiscellaneousSettings.Default)]
        partial class Union { }
        """)]
    [InlineData(
        """
        using RhoMicro.CodeAnalysis;
        using System;
        
        [UnionType<Int32, String>]
        [UnionTypeSettings(Miscellaneous = MiscellaneousSettings.None)]
        partial class Union { }
        """)]
    [InlineData(
        """
        using RhoMicro.CodeAnalysis;
        using System;
        
        [UnionType<Int32, String>]
        [UnionTypeSettings]
        partial class Union { }
        """)]
    [InlineData(
        """
        using RhoMicro.CodeAnalysis;
        using System;
        
        [assembly: UnionTypeSettings(Miscellaneous = MiscellaneousSettings.None)]
        
        [UnionType<Int32, String>]
        partial class Union { }
        """)]
    [InlineData(
        """
        using RhoMicro.CodeAnalysis;
        using System;
        
        [assembly: UnionTypeSettings(Miscellaneous = MiscellaneousSettings.Default)]

        [UnionType<Int32, String>]
        partial class Union { }
        """)]
    [InlineData(
        """
        using RhoMicro.CodeAnalysis;
        using System;
        
        [assembly: UnionTypeSettings]
        
        [UnionType<Int32, String>]
        partial class Union { }
        """)]
    public void OmitsJsonConverter(String source) =>
        TestUnionType(source, t =>
        {
            var omitted = t.GetTypeMembers().None(t =>
                t.Name == "JsonConverter");

            Assert.True(omitted);
        });
    [Theory]
    [InlineData(
        """
        using RhoMicro.CodeAnalysis;
        using System;

        [UnionType<Int32, String>]
        [UnionTypeSettings(Miscellaneous = MiscellaneousSettings.GenerateJsonConverter)]
        partial class Union { }
        """)]
    [InlineData(
        """
        using RhoMicro.CodeAnalysis;
        using System;
        
        [assembly: UnionTypeSettings(Miscellaneous = MiscellaneousSettings.GenerateJsonConverter)]
        
        [UnionType<Int32, String>]
        partial class Union { }
        """)]
    public void IncludesJsonConverter(String source) =>
        TestUnionType(source, t =>
        {
            var included = t.GetTypeMembers().One(t =>
                t.Name == "JsonConverter");

            Assert.True(included);
        });
}
