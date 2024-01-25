namespace RhoMicro.CodeAnalysis.Library.Text;

sealed class IndentedStringBuilderAppendable(Action<IndentedStringBuilder> strategy) : IIndentedStringBuilderAppendable
{
    public void AppendTo(IndentedStringBuilder builder) => strategy.Invoke(builder);
}