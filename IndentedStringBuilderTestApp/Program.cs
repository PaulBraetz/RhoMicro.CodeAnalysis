namespace IndentedStringBuilderTestApp
{

    using RhoMicro.CodeAnalysis.Library.Text;

    using static RhoMicro.CodeAnalysis.Library.Text.IndentedStringBuilder.Appendable;

    internal class Program
    {
        static void Main(String[] _0)
        {
            var operators = new IndentedStringBuilder().GetOperators(default);
            _ = operators + "Hello, World!" + NewLine +
                "Here is a new line :)" +
                OpenBraces +
                "inside a block now..." +
                CloseBlock +
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
}

namespace RhoMicro.CodeAnalysis.Library.Text
{
    partial class IndentedStringBuilder : IEquatable<IndentedStringBuilder?>
    {
        #region Open Block
        public IndentedStringBuilder OpenBlock(Block block)
        {
            OpenBlockCore(block);
            return this;
        }
        public IndentedStringBuilder OpenBracketsBlock() => OpenBlock(Blocks.Brackets);
        public IndentedStringBuilder OpenIndentBlock() => OpenBlock(Blocks.Indent);
        public IndentedStringBuilder OpenBracesBlock() => OpenBlock(Blocks.Braces);
        public IndentedStringBuilder OpenParensBlock() => OpenBlock(Blocks.Parens);
        public IndentedStringBuilder OpenAngledBlock() => OpenBlock(Blocks.Angled);
        public IndentedStringBuilder OpenRegionBlock(String name) => OpenBlock(Blocks.Region(name));
        #endregion
        #region Close Block
        public IndentedStringBuilder CloseBlock()
        {
            if(_blocks.Count == 0)
                return this;

            CloseBlockCore();
            return this;
        }
        public IndentedStringBuilder CloseAllBlocks()
        {
            while(_blocks.Count > 0)
            {
                CloseBlockCore();
            }

            return this;
        }
        #endregion
        #region Indent
        public IndentedStringBuilder Indent()
        {
            IndentCore();
            return this;
        }
        #endregion
        #region Detent
        public IndentedStringBuilder Detent()
        {
            DetentCore();
            return this;
        }
        #endregion
        #region Append Join
        public IndentedStringBuilder AppendJoin(IEnumerable<String> values) => AppendJoin(", ", values);
        public IndentedStringBuilder AppendJoin(StringOrChar separator, IEnumerable<String> values)
        {
            var enumerator = values.GetEnumerator();
            if(!enumerator.MoveNext())
                return this;

            AppendCore(enumerator.Current);

            while(enumerator.MoveNext())
            {
                AppendCore(separator, default);
                AppendCore(enumerator.Current);
            }

            return this;
        }
        public IndentedStringBuilder AppendJoinLines(IEnumerable<String> values) => AppendJoinLines(',', values);
        public IndentedStringBuilder AppendJoinLines(StringOrChar separator, IEnumerable<String> values)
        {
            var enumerator = values.GetEnumerator();
            if(!enumerator.MoveNext())
                return this;

            AppendCore(enumerator.Current);

            while(enumerator.MoveNext())
            {
                AppendCore(separator, default);
                AppendLineCore();
                AppendCore(enumerator.Current);
            }

            return this;
        }
        public IndentedStringBuilder AppendJoin(IEnumerable<Char> values) => AppendJoin(", ", values);
        public IndentedStringBuilder AppendJoin(StringOrChar separator, IEnumerable<Char> values)
        {
            var enumerator = values.GetEnumerator();
            if(!enumerator.MoveNext())
                return this;

            AppendCore(enumerator.Current);

            while(enumerator.MoveNext())
            {
                AppendCore(separator, default);
                AppendCore(enumerator.Current);
            }

            return this;
        }
        public IndentedStringBuilder AppendJoinLines(IEnumerable<Char> values) => AppendJoinLines(',', values);
        public IndentedStringBuilder AppendJoinLines(StringOrChar separator, IEnumerable<Char> values)
        {
            var enumerator = values.GetEnumerator();
            if(!enumerator.MoveNext())
                return this;

            AppendCore(enumerator.Current);

            while(enumerator.MoveNext())
            {
                AppendCore(separator, default);
                AppendLineCore();
                AppendCore(enumerator.Current);
            }

            return this;
        }
        public IndentedStringBuilder AppendJoin<T>(IEnumerable<T> values, CancellationToken cancellationToken)
            where T : IIndentedStringBuilderAppendable
            => AppendJoin(", ", values, cancellationToken);
        public IndentedStringBuilder AppendJoin<T>(StringOrChar separator, IEnumerable<T> values, CancellationToken cancellationToken)
            where T : IIndentedStringBuilderAppendable
        {
            cancellationToken.ThrowIfCancellationRequested();

            var enumerator = values.GetEnumerator();
            if(!enumerator.MoveNext())
                return this;

            AppendCore(enumerator.Current, cancellationToken);

            while(enumerator.MoveNext())
            {
                cancellationToken.ThrowIfCancellationRequested();

                AppendCore(separator, cancellationToken);
                AppendCore(enumerator.Current, cancellationToken);
            }

            return this;
        }
        public IndentedStringBuilder AppendJoinLines<T>(IEnumerable<T> values, CancellationToken cancellationToken)
            where T : IIndentedStringBuilderAppendable
            => AppendJoinLines(',', values, cancellationToken);
        public IndentedStringBuilder AppendJoinLines<T>(StringOrChar separator, IEnumerable<T> values, CancellationToken cancellationToken)
            where T : IIndentedStringBuilderAppendable
        {
            cancellationToken.ThrowIfCancellationRequested();

            var enumerator = values.GetEnumerator();
            if(!enumerator.MoveNext())
                return this;

            AppendCore(enumerator.Current, cancellationToken);

            while(enumerator.MoveNext())
            {
                cancellationToken.ThrowIfCancellationRequested();

                AppendCore(separator, cancellationToken);
                AppendLineCore();
                AppendCore(enumerator.Current, cancellationToken);
            }

            return this;
        }
        #endregion
        #region Append Line
        public IndentedStringBuilder AppendLine()
        {
            AppendLineCore();
            return this;
        }
        public IndentedStringBuilder AppendLine(String value) => Append(value).AppendLine();
        public IndentedStringBuilder AppendLine(Char value) => Append(value).AppendLine();
        public IndentedStringBuilder AppendLine<T>(T value, CancellationToken cancellationToken)
            where T : IIndentedStringBuilderAppendable
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Append(value, cancellationToken).AppendLine();
        }
        #endregion
        #region ToString & Equality
        public override String ToString() => _builder.ToString();
        public override Boolean Equals(Object? obj) => Equals(obj as IndentedStringBuilder);
        public Boolean Equals(IndentedStringBuilder? other) =>
            other is not null &&
            CollectionEqualityComparer<StringOrChar>.Default.Equals(_indentations, other._indentations) &&
            CollectionEqualityComparer<Block>.Default.Equals(_blocks, other._blocks) &&
            EqualityComparer<StringBuilder>.Default.Equals(_builder, other._builder);
        public override Int32 GetHashCode() =>
            (
                _builder,
                CollectionEqualityComparer<StringOrChar>.Default.GetHashCode(_indentations),
                CollectionEqualityComparer<Block>.Default.GetHashCode(_blocks)
            ).GetHashCode();
        #endregion
    }
}