namespace RhoMicro.CodeAnalysis.UnionsGenerator.Generators.Expansions;

using RhoMicro.CodeAnalysis.UtilityGenerators.Library;
using RhoMicro.CodeAnalysis.UnionsGenerator.Models;

using System.Threading;

sealed class NestedTypes(TargetDataModel model)
    : ExpansionBase(model, Macro.NestedClasses)
{
    protected override void Expand(ExpandingMacroBuilder builder)
    {
        _ = builder %
            "#region Nested Types";

        var representableTypes = Model.Annotations.AllRepresentableTypes;
        if(representableTypes.Count > 1)
        {
            _ = (builder *
                (Docs.Summary, b => _ = b * "Defines tags to discriminate between representable types.") *
                ConstantSources.EditorBrowsableNever /
                "enum " * Model.TagTypeName % " : Byte {")
                .AppendJoin(
                    ',',
                    representableTypes,
                    (b, a, t) => b.WithOperators(t) *
                        (Docs.Summary, b => _ = b *
                        "Used when representing an instance of " * a.DocCommentRef * '.') *
                        a.Names.SafeAlias)
                .Append('}');
        }

        var host = new StrategySourceHost(Model);
        representableTypes.ForEach(t => t.Storage.Visit(host));

        _ = builder *
            host.ValueTypeContainerType %
            "#endregion";
    }
}
