namespace RhoMicro.CodeAnalysis.Library.Text;

sealed class BlockScopeCollection : IDisposable
{
    private readonly List<BlockScope> _scopes = [];
    public void AddScope(BlockScope scope) => _scopes.Add(scope);
    public void Dispose()
    {
        foreach(var scope in _scopes)
            scope.Dispose();
    }
}
