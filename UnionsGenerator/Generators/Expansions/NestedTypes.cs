namespace RhoMicro.CodeAnalysis.UnionsGenerator.Generators.Expansions;

using RhoMicro.CodeAnalysis.UtilityGenerators.Library;
using RhoMicro.CodeAnalysis.UnionsGenerator.Models;

using System.Threading;

sealed class NestedTypes(TargetDataModel model)
    : ExpansionBase(model, Macro.NestedClasses)
{
    protected override void Expand(ExpandingMacroBuilder builder)
    {
        _ = builder +
            "#region Nested Types";

        var representableTypes = Model.Annotations.AllRepresentableTypes;
        if(representableTypes.Count > 1)
        {
            _ = (builder -
                """
                /// <summary>Defines tags to discriminate between representable types.</summary>
                [global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
                enum 
                """ +
                Model.TagTypeName -
                " : Byte {")
                .AppendJoin(
                    ',',
                    representableTypes,
                    (b, a, t) => b.GetOperators<Macro, TargetDataModel>(builder.CancellationToken) +
                        "/// <summary>" +
                        "Used when representing an instance of " +
                        a.DocCommentRef -
                        ".</summary>" -
                        a.Names.SafeAlias)
                .Append('}');
        }

        var host = new StrategySourceHost(Model);
        representableTypes.ForEach(t => t.Storage.Visit(host));

        _ = builder +
            host.ValueTypeContainerTypeAppendix -
            "#endregion";
    }
}
