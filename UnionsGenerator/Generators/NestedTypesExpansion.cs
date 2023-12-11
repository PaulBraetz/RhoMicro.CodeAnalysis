namespace RhoMicro.CodeAnalysis.UnionsGenerator.Generators;

using RhoMicro.CodeAnalysis.UtilityGenerators.Library;
using RhoMicro.CodeAnalysis.UnionsGenerator.Models;

using System.Threading;

sealed class NestedTypesExpansion(TargetDataModel model)
    : ExpansionBase(model, Macro.NestedClasses)
{
    public override void Expand(IExpandingMacroStringBuilder<Macro> builder, CancellationToken cancellationToken)
    {
        _ = builder.AppendLine("#region Nested Types");

        var representableTypes = Model.Annotations.AllRepresentableTypes;
        if(representableTypes.Count > 1)
        {
            _ = builder
                .AppendLine("[global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]")
                .AppendLine("private enum Tag : Byte {")
                .AppendJoin(',', representableTypes.Select(a => a.Names.SafeAlias))
                .Append('}');
        }

        var host = new StrategySourceHost(Model);
        representableTypes.ForEach(t => t.Storage.Visit(host));
        host.AppendValueTypeContainerType(builder);
        _ = builder.AppendLine("#endregion");
    }
}
