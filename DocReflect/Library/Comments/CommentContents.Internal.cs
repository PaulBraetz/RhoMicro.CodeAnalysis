namespace RhoMicro.CodeAnalysis.DocReflect.Comments;

using RhoMicro.CodeAnalysis.Library.Text;

using static RhoMicro.CodeAnalysis.Library.Text.IndentedStringBuilder.Appendables;

partial record struct CommentContents : IIndentedStringBuilderAppendable
{
    void IIndentedStringBuilderAppendable.AppendTo(IndentedStringBuilder builder) => _ = builder.Operators +
        "new " + typeof(CommentContents).FullName + '(' + NewLine + "\"\"\"" + NewLine + Text + NewLine + "\"\"\"" + ')';
}
