
namespace RhoMicro.CodeAnalysis.UnionsGenerator._Transformation.Visitors;

using RhoMicro.CodeAnalysis.Library.Text;
using RhoMicro.CodeAnalysis.UnionsGenerator._Models;

sealed class SourceTextVisitor(IndentedStringBuilder builder) : IVisitor<UnionTypeModel>
{
    private readonly IndentedStringBuilder _builder = builder;

    public void Visit(UnionTypeModel model) => _builder.Append(AppendableSourceText.Create(model));

    public override String ToString() => _builder.ToString();
}
