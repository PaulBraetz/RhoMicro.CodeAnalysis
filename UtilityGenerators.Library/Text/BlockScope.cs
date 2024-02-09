namespace RhoMicro.CodeAnalysis.Library.Text;
readonly struct BlockScope(IndentedStringBuilder builder) : IDisposable
{
    private readonly IndentedStringBuilder _builder = builder;
    public void Dispose() => _builder.CloseBlock();
}