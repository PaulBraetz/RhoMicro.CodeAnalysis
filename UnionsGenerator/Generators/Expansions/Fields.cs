namespace RhoMicro.CodeAnalysis.UnionsGenerator.Generators.Expansions;

using RhoMicro.CodeAnalysis.UtilityGenerators.Library;
using RhoMicro.CodeAnalysis.UnionsGenerator.Models;

using System.Threading;

sealed class Fields(TargetDataModel model)
    : ExpansionBase(model, Macro.Fields)
{
    public override void Expand(ExpandingMacroBuilder builder)
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
        _ = builder.Append(host.ReferenceTypeContainerFieldAppendix, cancellationToken)
            .Append(host.DedicatedReferenceFieldsAppendix, cancellationToken)
            .Append(host.ValueTypeContainerFieldAppendix, cancellationToken)
            .Append(host.DedicatedPureValueTypeFieldsAppendix, cancellationToken)
            .Append(host.AppendDedicatedImpureAndUnknownFields, cancellationToken);

        if(representableTypes.Count > 1)
        {
            _ = builder.AppendLine("[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]") *
                "private readonly ").Append(model.TagTypeName).AppendLine(" __tag;";
        }

        _ = builder.AppendLine("#endregion");
    }
}
