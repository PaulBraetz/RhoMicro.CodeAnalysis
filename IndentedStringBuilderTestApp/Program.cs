namespace IndentedStringBuilderTestApp;

using RhoMicro.CodeAnalysis.Library.Text;

using static RhoMicro.CodeAnalysis.Library.Text.IndentedStringBuilder.Appendables;

internal class Program
{
    static void Main(String[] _0)
    {
        //confusing: mutating operand instead of returning mutated instance
        var operators = new IndentedStringBuilder().Operators +
            "Hello, World!" + AppendLine() +
            "Here is a new line :)" +
            OpenBracesBlock() +
            "inside a block now..." +
            CloseBlock() +
            "and done :)";

        var result = operators.Builder.ToString();

        Console.WriteLine(result);
    }

    private static void Test1()
    {
        var builder = new IndentedStringBuilder(
                        IndentedStringBuilderOptions.Default with { DefaultIndentation = "  " })
                        .Append("namespace ").Append("Some.Parameter.Namespace")
                        .OpenBracesBlock();
        _ = builder.Append("partial class ").Append("SomeParameterName")
            .OpenBracesBlock()
            .Comment.OpenSummary()
            .Append("Sample method")
            .CloseBlock()
            .Comment.OpenParam("a")
            .Append("Sample Argument.")
            .CloseBlock()
            .Append("public void Foo(Int32 a)")
            .OpenBracesBlock()
            .Append("String[] bar = ")
            .OpenBracketsBlock()
            .Append("\"foobar\"")
            .CloseBlock()
            .AppendLine(';');

        using(builder.OpenBracesBlockScope())
        {
            _ = builder.Append("_ = foobar();");
        }

        _ = builder
            .AppendLine("throw new NotImplementedException();")
            .CloseBlock()
            .AppendLine("void Bar()");

        using(builder.OpenBracesBlockScope())
        {
            using(builder.CreateIndentScope())
            {
                _ = builder.OpenBracesBlock().CloseBlock().Append("throw new NotImplementedException();");
            }
        }

        var result = builder
            .CloseAllBlocks()
            .ToString();

        Console.WriteLine(result);
    }
}
