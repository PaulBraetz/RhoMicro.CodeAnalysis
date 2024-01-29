namespace RhoMicro.CodeAnalysis;

using RhoMicro.CodeAnalysis.UnionsGenerator._Models;

using System.Collections.Immutable;

partial class UnionTypeBaseAttribute
{
    public UnionTypeModel GetModel(TypeOrTypeParameterType type, CancellationToken ct)
    {
        var result = UnionTypeModel.Create(Alias, Options, Groups.ToImmutableArray(), type, ct);

        return result;
    }
}
