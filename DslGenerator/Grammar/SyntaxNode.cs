namespace RhoMicro.CodeAnalysis.DslGenerator.Grammar;

using RhoMicro.CodeAnalysis.Library.Text;

using System.Text.RegularExpressions;

#if DSL_GENERATOR
[IncludeFile]
#endif
abstract record SyntaxNode
{
    private static readonly Regex _quotePattern = new("(\"*)", RegexOptions.Compiled);
    protected static IndentedStringBuilder AppendCtorArg(
        IndentedStringBuilder builder,
        String argName,
        String argValue,
        Boolean quoteValue,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        _ = builder.Append(argName).Append(": ");

        var rawQuotes = quoteValue ?
            String.Concat(Enumerable.Repeat('"', _quotePattern.Matches(argValue)
                .Cast<Match>()
                .Select(m => m.Length)
                .Append(2)
                .Max() + 1)) :
            null;

        if(quoteValue)
        {
            _ = builder.Append('\n').Append(rawQuotes).Append('\n');
        }

        _ = builder.Append(argValue);

        if(quoteValue)
        {
            _ = builder.Append('\n').Append(rawQuotes).Append('\n');
        }

        return builder;
    }
    protected static IndentedStringBuilder AppendCtorArg(
        IndentedStringBuilder builder,
        String argName,
        SyntaxNode argValue,
        CancellationToken cancellationToken) =>
        builder.Append(argName).Append(": ").AppendMetaString(argValue, cancellationToken);
    protected static IndentedStringBuilder AppendCtorArg(
        IndentedStringBuilder builder,
        String argName,
        IEnumerable<SyntaxNode> argValue,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        _ = builder.Append(argName).Append(": [");
        var enumerator = argValue.GetEnumerator();
        if(enumerator.MoveNext())
        {
            var first = enumerator.Current;
            _ = builder.AppendMetaString(first, cancellationToken);
            while(enumerator.MoveNext())
            {
                cancellationToken.ThrowIfCancellationRequested();

                var next = enumerator.Current;
                _ = builder.Append(", ").AppendMetaString(next, cancellationToken);
            }
        } else
        {
            _ = builder.Append(' ');
        }

        _ = builder.Append(']');

        return builder;
    }

    public String ToDisplayString(CancellationToken cancellationToken) =>
        new IndentedStringBuilder().AppendDisplayString(this, cancellationToken).ToString();
    public String ToMetaString(CancellationToken cancellationToken) =>
        new IndentedStringBuilder().AppendMetaString(this, cancellationToken).ToString();
    public void AppendMetaStringTo(IndentedStringBuilder builder, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var type = GetType();
        _ = builder.Append("new ").Append(type.Namespace);
        append(type);
        _ = builder.Append('(');
        AppendCtorArgs(builder, cancellationToken);
        _ = builder.Append(')');

        void append(Type type)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if(type.DeclaringType != null)
            {
                append(type.DeclaringType);
            }

            _ = builder.Append('.').Append(type.Name);
        }
    }
    protected abstract void AppendCtorArgs(IndentedStringBuilder builder, CancellationToken cancellationToken);
    public abstract void AppendDisplayStringTo(IndentedStringBuilder builder, CancellationToken cancellationToken);
    public override String ToString() => ToDisplayString(default);
}
