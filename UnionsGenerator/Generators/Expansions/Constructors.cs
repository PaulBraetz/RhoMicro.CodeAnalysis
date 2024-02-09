namespace RhoMicro.CodeAnalysis.UnionsGenerator.Generators.Expansions;

using RhoMicro.CodeAnalysis.Library;
using RhoMicro.CodeAnalysis.UnionsGenerator.Models;

internal sealed class Constructors(TargetDataModel model) : ExpansionBase(model, Macro.Constructors)
{
    protected override void Expand(ExpandingMacroBuilder builder)
    => _ = (builder % "#region Constructors")
            .AppendJoin(Model.Annotations.AllRepresentableTypes, (b, e, t) =>
            {
                var ops = b.WithOperators(builder.CancellationToken);
                var accessibility = Model.GetSpecificAccessibility(e);

                _ = ops * accessibility * ' ' * Model.Symbol.Name * '(' * e.Names.FullTypeName % " value){";

                if(Model.Annotations.AllRepresentableTypes.Count > 1)
                    _ = ops * "__tag = " * e.GetCorrespondingTag(Model) % ';';

                _ = ops * (b => e.Storage.InstanceVariableAssignmentExpression(b, "value", "this")) % ";}";

                return ops;
            }, builder.CancellationToken)
        .WithOperators(builder.CancellationToken) % "#endregion";
}
