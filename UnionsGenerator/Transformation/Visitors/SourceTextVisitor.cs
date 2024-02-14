namespace RhoMicro.CodeAnalysis.UnionsGenerator.Transformation.Visitors;

using RhoMicro.CodeAnalysis.Library.Text;
using RhoMicro.CodeAnalysis.UnionsGenerator.Models;

sealed class SourceTextVisitor(IndentedStringBuilder builder) : IVisitor<UnionTypeModel>
{
    private readonly IndentedStringBuilder _builder = builder;

    public void Visit(UnionTypeModel model) => _builder.Append(new AppendableSourceText(model));

    public override String ToString() => _builder.ToString();
}
