namespace RhoMicro.CodeAnalysis.DocReflect.Comments;

using RhoMicro.CodeAnalysis.Library.Text;

partial record DocumentationComment : IIndentedStringBuilderAppendable
{
    void IIndentedStringBuilderAppendable.AppendTo(IndentedStringBuilder builder) => _ = builder.Operators +
       "new " + typeof(DocumentationComment).FullName + '(' + Contents + ')';
}
