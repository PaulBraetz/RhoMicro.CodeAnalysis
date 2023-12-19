namespace RhoMicro.CodeAnalysis.UnionsGenerator.Generators.Expansions;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using RhoMicro.CodeAnalysis.UtilityGenerators.Library;
using RhoMicro.CodeAnalysis.UnionsGenerator.Models;
using System.Text;

sealed class Head(TargetDataModel model)
    : ExpansionBase(model, Macro.Head)
{
    public static void ContainingClassHead(ExpandingMacroBuilder builder, ITypeSymbol nestedType)
    {
        _ = getContainingTypes(nestedType)
            .Select(s => (Kind: s.TypeKind switch
            {
                TypeKind.Class => "class ",
                TypeKind.Struct => "struct ",
                TypeKind.Interface => "interface ",
                _ => null
            }, Symbol: s))
            .Where(t => t.Kind != null)
            .Aggregate(
                builder,
                (b, t) => b * "partial " * t.Kind * ' ' * t.Symbol.ToMinimalOpenString() * '{');

        static IEnumerable<ITypeSymbol> getContainingTypes(ITypeSymbol symbol)
        {
            if(symbol.ContainingType != null)
                return getContainingTypes(symbol.ContainingType).Append(symbol.ContainingType);

            return Array.Empty<ITypeSymbol>();
        }
    }
    protected override void Expand(ExpandingMacroBuilder builder)
    {
        if(!Model.Symbol.ContainingNamespace.IsGlobalNamespace)
        {
            _ = builder *
                "namespace " *
                Model.Symbol.ContainingNamespace.ToDisplayString(
                    SymbolDisplayFormat.FullyQualifiedFormat
                    .WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted)) /
                '{';
        }

        _ = builder / (ContainingClassHead, Model.Symbol);

        if(Model.Annotations.Settings.Layout == LayoutSetting.Small && !Model.Symbol.IsGenericType)
            _ = builder / String.Empty % "[global::System.Runtime.InteropServices.StructLayout(global::System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 1)]";

        _ = builder *
            SyntaxFacts.GetText(Model.Symbol.DeclaredAccessibility) *
            " partial " *
            (Model.Symbol.IsValueType ?
                "struct " :
                "class ") %
            Model.Symbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat
            .WithGenericsOptions(SymbolDisplayGenericsOptions.IncludeTypeParameters)) *
            ": global::System.IEquatable<" *
            Model.Symbol.ToMinimalOpenString() *
            '>' %
            '{';
    }
}
