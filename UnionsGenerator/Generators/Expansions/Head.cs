namespace RhoMicro.CodeAnalysis.UnionsGenerator.Generators.Expansions;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using RhoMicro.CodeAnalysis.UtilityGenerators.Library;
using RhoMicro.CodeAnalysis.UnionsGenerator.Models;

sealed class Head(TargetDataModel model)
    : ExpansionBase(model, Macro.Head)
{
    protected override void Expand(ExpandingMacroBuilder builder)
    {
        if(!Model.Symbol.ContainingNamespace.IsGlobalNamespace)
        {
            _ = builder +
                "namespace " +
                Model.Symbol.ContainingNamespace.ToDisplayString(
                    SymbolDisplayFormat.FullyQualifiedFormat
                    .WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted)) -
                '{';
        }

        _ = builder - Model.Symbol.GetContainingClassHead();

        if(Model.Annotations.Settings.Layout == LayoutSetting.Small && !Model.Symbol.IsGenericType)
            _ = builder - "[global::System.Runtime.InteropServices.StructLayout(global::System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 1)]";

        _ = builder +
            SyntaxFacts.GetText(Model.Symbol.DeclaredAccessibility) +
            " partial " +
            (Model.Symbol.IsValueType ?
                "struct " :
                "class ") -
            Model.Symbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat
            .WithGenericsOptions(SymbolDisplayGenericsOptions.IncludeTypeParameters)) +
            AppendInterfaceImplementations -
            '{';
    }

    private void AppendInterfaceImplementations(ExpandingMacroBuilder builder)
    {
        var attributes = Model.Annotations;
        var target = Model.Symbol;

        _ = builder +
            ": global::System.IEquatable<" +
            target.ToMinimalOpenString() +
            '>';

        if(!(attributes.AllRepresentableTypes.Count == 1 &&
             attributes.AllRepresentableTypes[0].Attribute.Options.HasFlag(UnionTypeOptions.ImplicitConversionIfSolitary)))
        {
            //var omissions = Model.OperatorOmissions.AllOmissions;

            //_ = builder.AppendJoin(
            //    attributes.AllRepresentableTypes
            //    .Where(a => !omissions.Contains(a) &&
            //                !a.Attribute.RepresentableTypeIsGenericParameter ||
            //                 a.Attribute.Options.HasFlag(UnionTypeOptions.SupersetOfParameter))
            //    .Select(a => a.Names.FullTypeName),
            //    (b, n, t) => b.AppendLine(',')
            //            .Append(" global::RhoMicro.CodeAnalysis.UnionsGenerator.Abstractions.ISuperset<")
            //            .Append(n)
            //            .Append(',')
            //            .AppendOpen(target)
            //            .Append('>'),
            //    cancellationToken);
        }
    }
}
