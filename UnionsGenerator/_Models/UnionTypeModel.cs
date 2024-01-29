namespace RhoMicro.CodeAnalysis.UnionsGenerator._Models;

using Microsoft.CodeAnalysis;

using RhoMicro.CodeAnalysis.UnionsGenerator.Generators.Expansions;

using System.Collections.Immutable;
using System.Threading;
sealed record UnionTypeModel(TargetTypeModel Target, String? Alias, UnionTypeOptions Options, EquatableList<String> Groups, TypeOrTypeParameterType Type)
{
    internal static UnionTypeModel Create(
        INamedTypeSymbol target,
        String? alias,
        UnionTypeOptions options,
        EquatableList<String> groups,
        TypeOrTypeParameterType type,
        CancellationToken cancellationToken)
    {
        //TODO: implement complex data extraction here and get rid of type prop
        cancellationToken.ThrowIfCancellationRequested();

        var targetModel = TargetTypeModel.Create(target, cancellationToken);
        var result = new UnionTypeModel(targetModel, alias, options, groups, type);

        return result;
    }
}
