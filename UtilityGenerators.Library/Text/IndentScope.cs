namespace RhoMicro.CodeAnalysis.Library.Text;

readonly struct IndentScope(IndentedStringBuilder builder) : IDisposable
{
    private readonly IndentedStringBuilder _builder = builder;
    public void Dispose() => _builder.Detent();
}
