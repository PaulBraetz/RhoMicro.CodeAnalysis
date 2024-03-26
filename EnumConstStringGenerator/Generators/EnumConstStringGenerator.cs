namespace RhoMicro.CodeAnalysis.EnumConstStringGenerator.Generators;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using RhoMicro.CodeAnalysis.Library;
using RhoMicro.CodeAnalysis.Library.Text;

/// <summary>
/// Generates constant string values for enum members.
/// </summary>
[Generator(LanguageNames.CSharp)]
public sealed class EnumConstStringGenerator : IIncrementalGenerator
{
    private const String _attributeSource =
        """
        namespace RhoMicro.CodeAnalysis;
        [global::System.AttributeUsage(global::System.AttributeTargets.Enum, AllowMultiple = false, Inherited = false)]
        internal sealed class GenerateMemberStringConstantsAttribute:global::System.Attribute;
        """;
    private const String _attributeHintName = "GenerateMemberStringConstantsAttribute";
    private const String _attributeMetadataName = "RhoMicro.CodeAnalysis.GenerateMemberStringConstantsAttribute";
    /// <inheritdoc/>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var provider = context.SyntaxProvider.ForAttributeWithMetadataName
            <(String @namespace, String name, EquatableList<(String name, String value)> values)?>(
            _attributeMetadataName,
            (node, ct) => node is EnumDeclarationSyntax,
            (ctx, ct) =>
            {
                ct.ThrowIfCancellationRequested();
                if(ctx.TargetSymbol is not INamedTypeSymbol { TypeKind: TypeKind.Enum } target)
                    return null;

                var members = target
                    .GetMembers()
                    .OfType<IFieldSymbol>();

                var pastDefault = false;
                var lastExplicitDefinition = 0M;

                var values = new List<(String name, String value)>();

                foreach(var member in members)
                {
                    if(!member.HasConstantValue)
                    {
                        pastDefault = true;
                    } else
                    {
                        var value = (member.Name, member.ConstantValue?.ToString() ?? String.Empty);
                        values.Add(value);
                        continue;
                    }

                    if(member.DeclaringSyntaxReferences.FirstOrDefault()
                        ?.GetSyntax(ct) is not EnumMemberDeclarationSyntax
                        {
                            EqualsValue.Value: LiteralExpressionSyntax
                            {
                                Token.ValueText: { } literalText
                            }
                        } || !Decimal.TryParse(literalText, out var parsedLiteral))
                    {
                        if(pastDefault)
                        {
                            lastExplicitDefinition++;
                            var value = (member.Name, lastExplicitDefinition.ToString());
                            values.Add(value);
                        }

                        continue;
                    }

                    values.Add((member.Name, literalText));
                    lastExplicitDefinition = parsedLiteral;
                }

                var @namespace = target.ContainingNamespace?.ToDisplayString(
                    SymbolDisplayFormat.FullyQualifiedFormat
                    .WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted))
                    ?? String.Empty;

                var result = (@namespace, name: target.Name, values: values.ToEquatableList(ct));

                return result;
            }).Where(t => t.HasValue)
            .Select((t, ct) =>
            {
                var (@namespace, name, values) = t!.Value;

                var builder = new IndentedStringBuilder(
                    IndentedStringBuilderOptions.GeneratedFile with { AmbientCancellationToken = ct });

                if(@namespace != String.Empty)
                    builder.Append("namespace ").Append(@namespace).Append(';').AppendLineCore();

                builder.Append("public static class ").Append(name).Append("Strings")
                .OpenBracesBlock()
                .Append(b =>
                {
                    foreach(var (name, value) in values)
                    {
                        b.Append("public const global::System.String ")
                            .Append(name)
                            .Append(" = \"")
                            .Append(value)
                            .Append("\";")
                            .AppendLineCore();
                    }
                })
                .CloseBlockCore();

                var hintname = $"{( @namespace.Replace('.', '_') is { Length: > 0 } nonEmpty ? $"{nonEmpty}_" : String.Empty )}{name}.g.cs";
                var source = builder.ToString();

                return (hintname, source);
            });

        context.RegisterSourceOutput(provider, (ctx, t) => ctx.AddSource(t.hintname, t.source));
        context.RegisterPostInitializationOutput(ctx => ctx.AddSource(_attributeHintName, _attributeSource));
    }
}
