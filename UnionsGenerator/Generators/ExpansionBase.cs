namespace RhoMicro.CodeAnalysis.UnionsGenerator.Generators;

using RhoMicro.CodeAnalysis.UtilityGenerators.Library;
using RhoMicro.CodeAnalysis.UnionsGenerator.Models;

abstract class ExpansionBase : MacroExpansion<Macro>
{
    protected ExpansionBase(TargetDataModel model, Macro macro)
        : base(macro)
        => Model = model;
    protected TargetDataModel Model { get; }
}
