namespace RhoMicro.CodeAnalysis.DslGenerator.Grammar;

using System.Text;
using System.Text.RegularExpressions;

#if DSL_GENERATOR
[IncludeFile]
#endif
abstract record SyntaxNode
{
    private static readonly Regex _quotePattern = new("(\"*)", RegexOptions.Compiled);
    protected static StringBuilder AppendCtorArg(StringBuilder builder, String argName, String argValue, Boolean quoteValue = false)
    {
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
    protected static StringBuilder AppendCtorArg(StringBuilder builder, String argName, SyntaxNode argValue) =>
        builder.Append(argName).Append(": ").AppendMetaString(argValue);
    protected static StringBuilder AppendCtorArg(StringBuilder builder, String argName, IEnumerable<SyntaxNode> argValue)
    {
        _ = builder.Append(argName).Append(": [");
        var enumerator = argValue.GetEnumerator();
        if(enumerator.MoveNext())
        {
            var first = enumerator.Current;
            _ = builder.AppendMetaString(first);
            while(enumerator.MoveNext())
            {
                var next = enumerator.Current;
                _ = builder.Append(", ").AppendMetaString(next);
            }
        } else
        {
            _ = builder.Append(' ');
        }

        _ = builder.Append(']');

        return builder;
    }

    public String ToDisplayString() => new StringBuilder().AppendDisplayString(this).ToString();
    public String ToMetaString() => new StringBuilder().AppendMetaString(this).ToString();
    public void AppendMetaStringTo(StringBuilder builder)
    {
        var type = GetType();
        _ = builder.Append("new ").Append(type.Namespace);
        append(type);
        _ = builder.Append('(');
        AppendCtorArgs(builder);
        _ = builder.Append(')');

        void append(Type type)
        {
            if(type.DeclaringType != null)
            {
                append(type.DeclaringType);
            }

            _ = builder.Append('.').Append(type.Name);
        }
    }
    protected abstract void AppendCtorArgs(StringBuilder builder);
    public abstract void AppendDisplayStringTo(StringBuilder builder);
    public override String ToString() => ToDisplayString();
}
