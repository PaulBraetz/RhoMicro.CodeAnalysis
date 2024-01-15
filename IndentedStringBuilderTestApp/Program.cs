namespace IndentedStringBuilderTestApp;

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

using RhoMicro.CodeAnalysis.Library;

internal class Program
{
    static void Main(String[] _0)
    {
        var builder = new IndentedStringBuilder(
            IndentedStringBuilderOptions.Default with { Indentation = "  " })
            .Append("namespace ").AppendLine("Some.Parameter.Namespace")
            .OpenBlock()
            .Append("partial class ").AppendLine("SomeParameterName")
            .OpenBlock()
            .Append("public void Foo()")
            .OpenBlock()
            .AppendLine("String[] bar = ")
            .OpenBlock('[', ']', true)
            .AppendLine("\"foobar\"")
            .CloseBlock();

        using(builder.OpenBlockScope())
        {
            _ = builder.AppendLine("_ = foobar();");
        }

        _ = builder
            .AppendLine("throw new NotImplementedException();")
            .CloseBlock()
            .AppendLine("void Bar()");

        using(builder.OpenBlockScope())
        {
            using(builder.CreateIndentScope())
            {
                _ = builder.OpenBlock().CloseBlock().Append("throw new NotImplementedException();");
            }
        }

        var result = builder
            .CloseAllBlocks()
            .ToString();

        Console.WriteLine(result);
    }
}
