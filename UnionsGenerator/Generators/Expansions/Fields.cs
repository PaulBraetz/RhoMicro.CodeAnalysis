namespace RhoMicro.CodeAnalysis.UnionsGenerator.Generators.Expansions;

using RhoMicro.CodeAnalysis.UtilityGenerators.Library;
using RhoMicro.CodeAnalysis.UnionsGenerator.Models;

using System.Threading;

sealed class Fields(TargetDataModel model) : ExpansionBase(model, Macro.Fields)
{
    protected override void Expand(ExpandingMacroBuilder builder)
    {
        var representableTypes = Model.Annotations.AllRepresentableTypes;
        var host = new StrategySourceHost(Model);
        representableTypes.ForEach(s => s.Storage.Visit(host));

        _ = builder.AppendLine("#region Fields");
        /*
        obj container
        ref types
        value container
        known pure value types
        known impure valuetypes
        unkown types
        tag;
        TODO: implement comprehensive sorting refactor (analysis of value type container required)
        */
        _ = builder % [host.ReferenceTypeContainerField,
            host.DedicatedReferenceFields,
            host.ValueTypeContainerField,
            host.DedicatedPureValueTypeFields,
            host.DedicatedImpureAndUnknownFields];

        if(representableTypes.Count > 1)
        {
            _ = builder * "[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]" /
                "private readonly " * Model.TagTypeName % " __tag;";
        }

        _ = builder % "#endregion";
    }
}
