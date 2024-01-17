namespace IndentedStringBuilderTestApp;

using RhoMicro.CodeAnalysis.Library.Text;

internal class Program
{
    static void Main(String[] _0)
    {
        var builder = new IndentedStringBuilder(
            IndentedStringBuilderOptions.Default with { Indentation = "  " })
            .Append("namespace ").Append("Some.Parameter.Namespace")
            .OpenBracesBlock();
        _ = builder.Append("partial class ").Append("SomeParameterName")
            .OpenBracesBlock()
            .OpenDocCommentBlock("summary")
            .Append("/// Sample method")
            .CloseBlock()
            .OpenDocCommentBlock("param", "name", "a")
            .Append("/// Sample Argument.")
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
