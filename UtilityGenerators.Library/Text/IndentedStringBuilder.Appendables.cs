namespace RhoMicro.CodeAnalysis.Library.Text
{
    partial class IndentedStringBuilder
    {
        public static partial class Appendables
        {
            public static IndentedStringBuilderAppendable OpenBlock(RhoMicro.CodeAnalysis.Library.Text.Block block) =>
                new((b, c) =>
                {
                    c.ThrowIfCancellationRequested();
                    b.OpenBlock(block);
                });
            public static IndentedStringBuilderAppendable OpenBracketsBlock { get; } =
                new((b, c) =>
                {
                    c.ThrowIfCancellationRequested();
                    b.OpenBracketsBlock();
                });
            public static IndentedStringBuilderAppendable OpenIndentBlock { get; } =
                new((b, c) =>
                {
                    c.ThrowIfCancellationRequested();
                    b.OpenIndentBlock();
                });
            public static IndentedStringBuilderAppendable OpenBracesBlock { get; } =
                new((b, c) =>
                {
                    c.ThrowIfCancellationRequested();
                    b.OpenBracesBlock();
                });
            public static IndentedStringBuilderAppendable OpenParensBlock { get; } =
                new((b, c) =>
                {
                    c.ThrowIfCancellationRequested();
                    b.OpenParensBlock();
                });
            public static IndentedStringBuilderAppendable OpenAngledBlock { get; } =
                new((b, c) =>
                {
                    c.ThrowIfCancellationRequested();
                    b.OpenAngledBlock();
                });
            public static IndentedStringBuilderAppendable OpenRegionBlock(string name) =>
                new((b, c) =>
                {
                    c.ThrowIfCancellationRequested();
                    b.OpenRegionBlock(name);
                });
            public static IndentedStringBuilderAppendable CloseBlock { get; } =
                new((b, c) =>
                {
                    c.ThrowIfCancellationRequested();
                    b.CloseBlock();
                });
            public static IndentedStringBuilderAppendable CloseAllBlocks { get; } =
                new((b, c) =>
                {
                    c.ThrowIfCancellationRequested();
                    b.CloseAllBlocks();
                });
            public static IndentedStringBuilderAppendable Indent { get; } =
                new((b, c) =>
                {
                    c.ThrowIfCancellationRequested();
                    b.Indent();
                });
            public static IndentedStringBuilderAppendable Detent { get; } =
                new((b, c) =>
                {
                    c.ThrowIfCancellationRequested();
                    b.Detent();
                });
            public static IndentedStringBuilderAppendable Join(System.Collections.Generic.IEnumerable<string> values) =>
                new((b, c) =>
                {
                    c.ThrowIfCancellationRequested();
                    b.AppendJoin(values);
                });
            public static IndentedStringBuilderAppendable Join(RhoMicro.CodeAnalysis.Library.Text.StringOrChar separator, System.Collections.Generic.IEnumerable<string> values) =>
                new((b, c) =>
                {
                    c.ThrowIfCancellationRequested();
                    b.AppendJoin(separator, values);
                });
            public static IndentedStringBuilderAppendable JoinLines(System.Collections.Generic.IEnumerable<string> values) =>
                new((b, c) =>
                {
                    c.ThrowIfCancellationRequested();
                    b.AppendJoinLines(values);
                });
            public static IndentedStringBuilderAppendable JoinLines(RhoMicro.CodeAnalysis.Library.Text.StringOrChar separator, System.Collections.Generic.IEnumerable<string> values) =>
                new((b, c) =>
                {
                    c.ThrowIfCancellationRequested();
                    b.AppendJoinLines(separator, values);
                });
            public static IndentedStringBuilderAppendable Join(System.Collections.Generic.IEnumerable<char> values) =>
                new((b, c) =>
                {
                    c.ThrowIfCancellationRequested();
                    b.AppendJoin(values);
                });
            public static IndentedStringBuilderAppendable Join(RhoMicro.CodeAnalysis.Library.Text.StringOrChar separator, System.Collections.Generic.IEnumerable<char> values) =>
                new((b, c) =>
                {
                    c.ThrowIfCancellationRequested();
                    b.AppendJoin(separator, values);
                });
            public static IndentedStringBuilderAppendable JoinLines(System.Collections.Generic.IEnumerable<char> values) =>
                new((b, c) =>
                {
                    c.ThrowIfCancellationRequested();
                    b.AppendJoinLines(values);
                });
            public static IndentedStringBuilderAppendable JoinLines(RhoMicro.CodeAnalysis.Library.Text.StringOrChar separator, System.Collections.Generic.IEnumerable<char> values) =>
                new((b, c) =>
                {
                    c.ThrowIfCancellationRequested();
                    b.AppendJoinLines(separator, values);
                });
            public static IndentedStringBuilderAppendable Join<T>(System.Collections.Generic.IEnumerable<T> values, System.Threading.CancellationToken cancellationToken)
where T : RhoMicro.CodeAnalysis.Library.Text.IIndentedStringBuilderAppendable =>
                new((b, c) =>
                {
                    c.ThrowIfCancellationRequested();
                    b.AppendJoin(values, cancellationToken);
                });
            public static IndentedStringBuilderAppendable Join<T>(RhoMicro.CodeAnalysis.Library.Text.StringOrChar separator, System.Collections.Generic.IEnumerable<T> values, System.Threading.CancellationToken cancellationToken)
where T : RhoMicro.CodeAnalysis.Library.Text.IIndentedStringBuilderAppendable =>
                new((b, c) =>
                {
                    c.ThrowIfCancellationRequested();
                    b.AppendJoin(separator, values, cancellationToken);
                });
            public static IndentedStringBuilderAppendable JoinLines<T>(System.Collections.Generic.IEnumerable<T> values, System.Threading.CancellationToken cancellationToken)
where T : RhoMicro.CodeAnalysis.Library.Text.IIndentedStringBuilderAppendable =>
                new((b, c) =>
                {
                    c.ThrowIfCancellationRequested();
                    b.AppendJoinLines(values, cancellationToken);
                });
            public static IndentedStringBuilderAppendable JoinLines<T>(RhoMicro.CodeAnalysis.Library.Text.StringOrChar separator, System.Collections.Generic.IEnumerable<T> values, System.Threading.CancellationToken cancellationToken)
where T : RhoMicro.CodeAnalysis.Library.Text.IIndentedStringBuilderAppendable =>
                new((b, c) =>
                {
                    c.ThrowIfCancellationRequested();
                    b.AppendJoinLines(separator, values, cancellationToken);
                });
            public static IndentedStringBuilderAppendable Line { get; } =
                new((b, c) =>
                {
                    c.ThrowIfCancellationRequested();
                    b.AppendLine();
                });
            public static IndentedStringBuilderAppendable Line(string value) =>
                new((b, c) =>
                {
                    c.ThrowIfCancellationRequested();
                    b.AppendLine(value);
                });
            public static IndentedStringBuilderAppendable Line(char value) =>
                new((b, c) =>
                {
                    c.ThrowIfCancellationRequested();
                    b.AppendLine(value);
                });
            public static IndentedStringBuilderAppendable Line<T>(T value, System.Threading.CancellationToken cancellationToken)
where T : RhoMicro.CodeAnalysis.Library.Text.IIndentedStringBuilderAppendable =>
                new((b, c) =>
                {
                    c.ThrowIfCancellationRequested();
                    b.AppendLine(value, cancellationToken);
                });
        }
    }
}
