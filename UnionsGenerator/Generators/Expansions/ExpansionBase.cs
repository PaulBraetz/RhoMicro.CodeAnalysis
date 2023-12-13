namespace RhoMicro.CodeAnalysis.UnionsGenerator.Generators.Expansions;

using RhoMicro.CodeAnalysis.UtilityGenerators.Library;
using RhoMicro.CodeAnalysis.UnionsGenerator.Models;
using System.Threading;

abstract class ExpansionBase : MacroExpansion<Macro>
{
    protected ExpansionBase(TargetDataModel model, Macro macro)
        : base(macro)
        => Model = model;
    protected TargetDataModel Model { get; }
    public sealed override void Expand(IExpandingMacroStringBuilder<Macro> builder, CancellationToken cancellationToken) =>
        Expand(builder.GetOperators<Macro, TargetDataModel>(cancellationToken));
    protected abstract void Expand(ExpandingMacroBuilder builder);
}
