namespace RhoMicro.CodeAnalysis.CopyToGenerator.MacroExpansions;

using RhoMicro.CodeAnalysis.CopyToGenerator;

abstract class MacroExpansionBase(Model model, Macro macro)
    : Common.MacroExpansion<Macro>(macro)
{
    protected Model Model { get; } = model;
}
