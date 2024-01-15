namespace RhoMicro.CodeAnalysis.Library.Text;
using System.Threading;

readonly record struct AttributePropertyArgumentModel(String Name, String Value) :
    IIndentedStringBuilderAppendable
{
    public void AppendTo(IndentedStringBuilder builder, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        _ = builder.Append(Name).Append(" = ").Append(Value);
    }
}
