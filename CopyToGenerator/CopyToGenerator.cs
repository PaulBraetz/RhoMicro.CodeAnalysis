namespace RhoMicro.CodeAnalysis.CopyToGenerator;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System;
using System.Diagnostics;
using System.Linq;
using System.Text;

[Generator]
public class CopyToGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var provider = context.SyntaxProvider
            .ForGenerateCopyToAttribute(
                (n, t) => n is ClassDeclarationSyntax c && c.Modifiers.Any(SyntaxKind.PartialKeyword),
                (c, t) =>
                {
                    var symbol = c.TargetSymbol as INamedTypeSymbol ??
                        throw new InvalidOperationException("Unable to retrieve target symbol as named type symbol.");

                    var props = symbol.GetMembers()
                        .OfType<IPropertySymbol>()
                        .Where(p => p.DeclaredAccessibility == Accessibility.Public &&
                                  p.SetMethod != null &&
                                  p.GetMethod != null &&
                                  p.SetMethod.DeclaredAccessibility == Accessibility.Public &&
                                  p.GetMethod.DeclaredAccessibility == Accessibility.Public);

                    var methodSourceBuilder = new StringBuilder()
                        .AppendLine("/// <summary>")
                        .AppendLine("/// Copies this instances public properties to another ones.")
                        .AppendLine("/// </summary>")
                        .AppendLine("/// <param name =\"target\">The instance to copy this instances properties' values to.</param>")
                        .Append("public void CopyTo(")
                        .Append(symbol.Name).Append(" target){if(this == target || target == null){ return;}if(AvoidCopy(this, target)){return;}");

                    var methodSource = props.Select(GetCopyStatement)
                        .Aggregate(methodSourceBuilder, (b, a) => b.Append(a))
                        .Append('}')
                        .ToString();

                    return new SourceModel(methodSource, symbol, String.Empty);
                })
            .Select((m, t) =>
            {
                var areValueEqualSourceBuilder = new StringBuilder()
                    .AppendLine("/// <summary>")
                    .Append("/// Evaluates whether copying between two instances of <see cref=\"").Append(m.Symbol.Name).AppendLine("\"/> should be avoided due to equivalence. This can help avoid unnecessary copying or infinite copying in nested recursive relationships.")
                    .AppendLine("/// </summary>")
                    .AppendLine("/// <param name =\"a\">The first instance to compare.</param>")
                    .AppendLine("/// <param name =\"b\">The second instance to compare.</param>")
                    .AppendLine("/// <param name =\"result\">Upon returning, contains <see langword=\"true\"/> if copying between <paramref name=\"a\"/> and <paramref name=\"b\"/> should be avoided; otherwise, <see langword=\"false\"/>.</param>")
                    .Append("static partial void AvoidCopy(")
                    .Append(m.Symbol.Name).Append(" a, ").Append(m.Symbol.Name).Append(" b, ref global::System.Boolean result);")
                    .AppendLine("/// <summary>")
                    .Append("/// Evaluates whether copying between two instances of <see cref=\"").Append(m.Symbol.Name).AppendLine("\"/> should be avoided due to equivalence. This can help avoid unnecessary copying or infinite copying in nested recursive relationships.")
                    .AppendLine("/// </summary>")
                    .AppendLine("/// <param name =\"a\">The first instance to compare.</param>")
                    .AppendLine("/// <param name =\"b\">The second instance to compare.</param>")
                    .AppendLine("/// <returns><see langword=\"true\"/> if copying between <paramref name=\"a\"/> and <paramref name=\"b\"/> should be avoided; otherwise, <see langword=\"false\"/>.</returns>")
                    .Append("static global::System.Boolean AvoidCopy(")
                    .Append(m.Symbol.Name).Append(" a, ").Append(m.Symbol.Name).Append(" b){{var result = false; AvoidCopy(a,b,ref result);return result;}}");

                var areValueEqualSource = areValueEqualSourceBuilder.ToString();

                return m.WithAreValueEqualSource(areValueEqualSource);
            })
            .Select((m, t) =>
            {
                var classSourceBuilder = new StringBuilder()
                    .AppendLine("#pragma warning disable");

                if(!m.Symbol.ContainingNamespace.IsGlobalNamespace)
                {
                    _ = classSourceBuilder.Append("namespace ")
                        .Append(m.Symbol.ContainingNamespace.ToDisplayString())
                        .Append('{');
                }

                _ = classSourceBuilder
                    .Append("partial class ").AppendLine(m.Symbol.Name)
                    .Append('{')
                    .AppendLine(m.AreValueEqualSource)
                    .Append(m.Source)
                    .Append('}');

                if(!m.Symbol.ContainingNamespace.IsGlobalNamespace)
                {
                    _ = classSourceBuilder.Append('}');
                }

                var source = classSourceBuilder.ToString();
                var formattedSource = CSharpSyntaxTree.ParseText(source, cancellationToken: t)
                    .GetRoot(t)
                    .NormalizeWhitespace()
                    .SyntaxTree
                    .GetText(t)
                    .ToString();

                return (Source: formattedSource, HintName: $"{m.Symbol.Name}.g.cs");
            });

        context.RegisterSourceOutput(provider, (c, output) => c.AddSource(output.HintName, output.Source));
        context.RegisterPostInitializationOutput(c => c.AddSource($"{nameof(GenerateCopyToAttribute)}.g.cs", GenerateCopyToAttribute.SourceText));
    }

    private static String GetCopyStatement(IPropertySymbol p)
    {
        var resultBuilder = new StringBuilder();

        if(p.Type.TryGetListAssignableItemType(out var itemType))
#pragma warning disable IDE0045 // Convert to conditional expression
        {
            _ = resultBuilder
                .Append("if(target.").Append(p.Name).Append(" == default || this.").Append(p.Name).Append(" == default){")
                .Append("target.").Append(p.Name).Append(" = this.").Append(p.Name).Append(";}else{")
                //tradeoff: duplicates are lost
                .Append("var oldTargetElements = new global::System.Collections.Generic.HashSet<")
                .Append(itemType!.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat))
                .Append(">(target.").Append(p.Name).Append(");")
                .Append("var newTargetElements = new global::System.Collections.Generic.List<")
                .Append(itemType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat))
                .Append(">();foreach(var sourceElement in ").Append(p.Name)
                .Append(
"""
)
{
    if(oldTargetElements.TryGetValue(sourceElement, out var targetElement))
    {
        //both target & source contain element -> copy
        sourceElement.CopyTo(targetElement);
        newTargetElements.Add(targetElement);
        _ = oldTargetElements.Remove(sourceElement);
    } else
    {
        //only source contains element -> add to target
        newTargetElements.Add(sourceElement);
    }
}

//add remaining target elements to result collection
foreach(var remainingElement in oldTargetElements)
{
    newTargetElements.Add(remainingElement);
}
target.
""")
                .Append(p.Name).Append(" = newTargetElements;}");

        } else if(p.Type.IsMarked())
        {
            _ = resultBuilder.Append("if(target.").Append(p.Name)
                .Append(" == default || this.").Append(p.Name).Append(" == default){")
                .Append("target.").Append(p.Name).Append(" = this.").Append(p.Name).Append(";}")
                .Append("else{")
                .Append("this.").Append(p.Name).Append(".CopyTo(target.").Append(p.Name).Append(");}");
        } else
        {
            _ = resultBuilder.Append("target.")
                .Append(p.Name)
                .Append(" = this.")
                .Append(p.Name)
                .Append(';');
        }
#pragma warning restore IDE0045 // Convert to conditional expression

        var result = resultBuilder.ToString();

        return result;
    }
}
