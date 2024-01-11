namespace RhoMicro.CodeAnalysis.DslGenerator.Grammar;

using System.Text;

#if DSL_GENERATOR
[IncludeFile]
#endif
static class StringBuilderExtensions
{
    public static StringBuilder AppendDisplayString(this StringBuilder builder, SyntaxNode node)
    {
        node.AppendDisplayStringTo(builder);
        return builder;
    }
    public static StringBuilder AppendMetaString(this StringBuilder builder, SyntaxNode node)
    {
        node.AppendMetaStringTo(builder);
        return builder;
    }
}