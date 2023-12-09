namespace RhoMicro.CodeAnalysis.UnionsGenerator.Generators;

using RhoMicro.CodeAnalysis.Common;
using RhoMicro.CodeAnalysis.UnionsGenerator.Models;

using System.Threading;

sealed class FieldsExpansion(TargetDataModel model)
    : ExpansionBase(model, Macro.Fields)
{
    public override void Expand(IExpandingMacroStringBuilder<Macro> builder, CancellationToken cancellationToken)
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
        host.AppendReferenceTypeContainerField(builder, cancellationToken);
        host.AppendDedicatedReferenceFields(builder, cancellationToken);
        host.AppendValueTypeContainerField(builder, cancellationToken);
        host.AppendDedicatedPureValueTypeFields(builder, cancellationToken);
        host.AppendDedicatedImpureAndUnknownFields(builder, cancellationToken);
        if(representableTypes.Count > 1)
        {
            _ = builder.AppendLine("[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]")
                .Append("private readonly Tag __tag;");
        }

        _ = builder.AppendLine("#endregion");
    }
}
