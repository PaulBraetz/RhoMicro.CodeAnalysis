namespace RhoMicro.CodeAnalysis.Library.Text;

sealed class IndentedStringBuilderAppendable(Action<IndentedStringBuilder, CancellationToken> strategy) : IIndentedStringBuilderAppendable
{
    public void AppendTo(IndentedStringBuilder builder, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        strategy.Invoke(builder, cancellationToken);
    }
}