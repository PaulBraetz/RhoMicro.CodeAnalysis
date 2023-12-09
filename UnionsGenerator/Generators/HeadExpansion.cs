namespace RhoMicro.CodeAnalysis.UnionsGenerator.Generators;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using RhoMicro.CodeAnalysis.Common;
using RhoMicro.CodeAnalysis.UnionsGenerator.Models;

sealed class HeadExpansion(TargetDataModel model)
    : ExpansionBase(model, Macro.Head)
{
    public override void Expand(IExpandingMacroStringBuilder<Macro> builder, CancellationToken cancellationToken)
    {
        if(!Model.Symbol.ContainingNamespace.IsGlobalNamespace)
        {
            _ = builder.Append("namespace ")
                .Append(Model.Symbol.ContainingNamespace.ToDisplayString(
                    SymbolDisplayFormat.FullyQualifiedFormat
                    .WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted)))
                .AppendLine('{');
        }

        _ = builder.AppendLine(Model.Symbol.GetContainingClassHead());

        if(Model.Annotations.Settings.Layout == LayoutSetting.Small && !Model.Symbol.IsGenericType)
            _ = builder.AppendLine("[global::System.Runtime.InteropServices.StructLayout(global::System.Runtime.InteropServices.LayoutKind.Sequential, Pack = 1)]");

        _ = builder.Append(SyntaxFacts.GetText(Model.Symbol.DeclaredAccessibility))
            .Append(" partial ")
            .Append(Model.Symbol.IsValueType ?
                "struct " :
                "class ")
            .AppendLine(Model.Symbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat
                .WithGenericsOptions(SymbolDisplayGenericsOptions.IncludeTypeParameters)));

        AppendInterfaceImplementations(builder, cancellationToken);

        _ = builder.AppendLine('{');
    }

    private void AppendInterfaceImplementations(
        IExpandingMacroStringBuilder<Macro> builder,
        CancellationToken cancellationToken)
    {
        var attributes = Model.Annotations;
        var target = Model.Symbol;
        var targetDeclaration = Model.TargetDeclaration;

        _ = builder.Append(": global::RhoMicro.CodeAnalysis.UnionsGenerator.Abstractions.IUnion<")
            .AppendOpen(target)
            .Append(',')
            .AppendJoin(
                attributes.AllRepresentableTypes.Select((a, i) => (Name: a.Names.FullTypeName, Index: i)),
                (b, n, t) => b.Append(n.Name).Append(n.Index != attributes.AllRepresentableTypes.Count - 1 ? "," : String.Empty),
                cancellationToken)
            .AppendLine(">,")
            .Append("global::System.IEquatable<")
            .AppendOpen(target)
            .Append('>');

        if(!(attributes.AllRepresentableTypes.Count == 1 &&
             attributes.AllRepresentableTypes[0].Attribute.Options.HasFlag(UnionTypeOptions.ImplicitConversionIfSolitary)))
        {
            var omissions = Model.OperatorOmissions.AllOmissions;

            _ = builder.AppendJoin(
                attributes.AllRepresentableTypes
                .Where(a => !omissions.Contains(a) &&
                            !a.Attribute.RepresentableTypeIsGenericParameter ||
                             a.Attribute.Options.HasFlag(UnionTypeOptions.SupersetOfParameter))
                .Select(a => a.Names.FullTypeName),
                (b, n, t) => b.AppendLine(',')
                        .Append(" global::RhoMicro.CodeAnalysis.UnionsGenerator.Abstractions.ISuperset<")
                        .Append(n)
                        .Append(',')
                        .AppendOpen(target)
                        .Append('>'),
                cancellationToken);
        }
    }
}
