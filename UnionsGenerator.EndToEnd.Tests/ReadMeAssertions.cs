#pragma warning disable IDE0250
#pragma warning disable IDE0059 
#pragma warning disable CS1591 
namespace RhoMicro.CodeAnalysis.UnionsGenerator.EndToEnd.Tests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

public partial class ReadMeAssertions
{
    [UnionType<Int32>]
    [UnionType<String>]
    partial struct IntOrString;
    [Fact]
    public void TypeDeclarationTarget()
    {
        IntOrString u = "Hello, World!"; //implicitly converted
        u = 32; //implicitly converted
    }

    partial struct GenericUnion<[UnionType] T0, [UnionType] T1>;
    [Fact]
    public void TypeParameterTarget()
    {
        var u = GenericUnion<Int32, String>.CreateFromT1("Hello, World!");
        u = GenericUnion<Int32, String>.CreateFromT0(32);
    }

    [UnionType<List<String>>(Alias = "MultipleNames")]
    [UnionType<String>(Alias = "SingleName")]
    partial struct Names;
    [Fact]
    public void AliasExample()
    {
        Names n = "John";
        if(n.IsSingleName)
        {
            var singleName = n.AsSingleName;
        } else if(n.IsMultipleNames)
        {
            var multipleNames = n.AsMultipleNames;
        }
    }

    [UnionType<Int32>(Options = UnionTypeOptions.ImplicitConversionIfSolitary)]
    partial struct Int32Alias;
    [Fact]
    public void Solitary()
    {
        var i = 32;
        Int32Alias u = i;
        i = u;
    }

    partial struct GenericConvertableUnion<[UnionType] T>;
    [Fact]
    public void SupersetOfParameter()
    {
        GenericConvertableUnion<Int32> u = 32;
        var i = u;
    }

#pragma warning disable CS8604 // Possible null reference argument.
    [UnionType<String>(Options = UnionTypeOptions.Nullable)]
    [UnionType<List<String>>]
    partial struct NullableStringUnion;
    [Fact]
    public void NullableUnion()
    {
        NullableStringUnion u = (String?)null;
        u = new List<String>();
        u = "Nonnull String";
        u = (List<String>?)null; //CS8604 - Possible null reference argument for parameter.
    }
#pragma warning restore CS8604 // Possible null reference argument.

    [UnionType<Int32, Single>(Groups = ["Number"])]
    [UnionType<String, Char>(Groups = ["Text"])]
    partial struct GroupedUnion;
    [Fact]
    public void GroupedUnions()
    {
        GroupedUnion u = "Hello, World!";
        if(u.IsNumberGroup)
        {
            Assert.Fail("Expected union to be text.");
        }
        if(!u.IsTextGroup)
        {
            Assert.Fail("Expected union to be text.");
        }

        u = 32f;
        if(!u.IsNumberGroup)
        {
            Assert.Fail("Expected union to be number.");
        }
        if(u.IsTextGroup)
        {
            Assert.Fail("Expected union to be number.");
        }
    }
}
