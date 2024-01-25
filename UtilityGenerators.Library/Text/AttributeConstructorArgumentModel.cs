namespace RhoMicro.CodeAnalysis.Library.Text;
readonly record struct AttributeConstructorArgumentModel(String Name, String Value) :
    IIndentedStringBuilderAppendable
{
    public void AppendTo(IndentedStringBuilder builder) => builder.Append(Name).Append(": ").Append(Value);
}
