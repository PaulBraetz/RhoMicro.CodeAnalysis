namespace RhoMicro.CodeAnalysis.Library.Tests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

public class OperatorDecoratorTests
{
    /*
    void B()
    {
        var instance = new A();

        //IdentifierName
        var t0 = (Bar, "", 0);
        var t1 = (Baz, "", 0);
        Action<StringBuilder, String, Int32> foobar = (b, s, i) => { };
        var t2 = (foobar, "", 0);
        void localFoobar(StringBuilder b, String s, Int32 i) { }
        var t3 = (localFoobar, "", 0);

        //SimpleMemberAccessExpression
        var t4 = (A.Bar, "", 0);
        var t5 = (instance.Baz, "", 0);
        var t6 = (this.Baz, "", 0);

        //ParenthesizedLambdaExpression
        var t7 = ((b, s, i) => { }, "", 0);

        //CastExpression
        var t8 = ((Action<StringBuilder, String, Int32>)((b, s, i) => { }), "", 0);
    }
    private readonly Action<StringBuilder, String, Int32> _del;
    static void Bar(StringBuilder builder, String value1, Int32 value2 = 0){}
    static void Bar(StringBuilder builder, String value1){}
    void Baz(StringBuilder builder, String value1 = "", Int32 value2){}
    void Baz(StringBuilder builder, String value1){}
    */

    [Fact]
    public void GeneratesOperators1()
    {
        const String expected = "Hello, World!";
        var builder = ExpandingMacroStringBuilder.Create<Int32>().WithOperators(default);
        _ = builder * (foo, expected);

        var actual = builder.Build();
        Assert.Equal(expected, actual);

        static void foo(ExpandingMacroStringBuilder.OperatorsDecorator<Int32> b, String a) => _ = b * a;
    }
    [Fact]
    public void GeneratesOperators2()
    {
        const String expected = "Hello, World!";
        var builder = ExpandingMacroStringBuilder.Create<Int32>().WithOperators(default);
        _ = builder * (foo, "Hello, World", "!");

        var actual = builder.Build();
        Assert.Equal(expected, actual);

        static void foo(ExpandingMacroStringBuilder.OperatorsDecorator<Int32> b, String a, String c) => _ = b * a * c;
    }
    [Fact]
    public void GeneratesOperators3()
    {
        const String expected = "Hello, World!";
        var builder = ExpandingMacroStringBuilder.Create<Int32>().WithOperators(default);
        _ = builder * (foo, "Hello, World", '!');

        var actual = builder.Build();
        Assert.Equal(expected, actual);

        static void foo(ExpandingMacroStringBuilder.OperatorsDecorator<Int32> b, String a, Char c) => _ = b * a * c;
    }
    [Fact]
    public void GeneratesOperators4()
    {
        const String expected = "Hello, World!";
        var builder = ExpandingMacroStringBuilder.Create<Int32>().WithOperators(default);
        _ = builder * (foo, $"Hello, {"World"}", '!', .5f);

        var actual = builder.Build();
        Assert.Equal(expected, actual);

        static void foo(ExpandingMacroStringBuilder.OperatorsDecorator<Int32> b, String a, Char c, Single d) => _ = b * a * c;
    }
    [Fact]
    public void GeneratesOperators5()
    {
        const String expected = "Hello,\n World!";
        var builder = ExpandingMacroStringBuilder.Create<Int32>().WithOperators(default);
        _ = builder * (bar, "Hello,", " World!");

        var actual = builder.Build();
        Assert.Equal(expected, actual);

        static void bar(ExpandingMacroStringBuilder.OperatorsDecorator<Int32> b, String a, String c) => _ = b * a / c;
    }
    [Fact]
    public void GeneratesOperators6()
    {
        const String expected = "Hello,\n World!";
        var builder = ExpandingMacroStringBuilder.Create<Int32>().WithOperators(default);
        _ = builder * (bar, "Hello,", " World!");

        var actual = builder.Build();
        Assert.Equal(expected, actual);

        static void bar(ExpandingMacroStringBuilder.OperatorsDecorator<Int32> b, String a, String c) => _ = b % a * c;
    }
    [Fact]
    public void GeneratesOperators7()
    {
        const String expected = "Hello,\n4";
        var builder = ExpandingMacroStringBuilder.Create<Int32>();
        _ = builder.WithOperators(default) * (bar, "Hello,", 4);

        var actual = builder.Build();
        Assert.Equal(expected, actual);

        static void bar(ExpandingMacroStringBuilder.OperatorsDecorator<Int32> b, String a, Byte c) => _ = b % a * c.ToString();
    }
}
