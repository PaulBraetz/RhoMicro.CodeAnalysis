
namespace RhoMicro.CodeAnalysis.UtilityGenerators.Library.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using RhoMicro.CodeAnalysis.Library.Text;
using RhoMicro.CodeAnalysis.Library;

using System;
using System.Collections.Immutable;
using System.Text;
using System.Threading;

using Signature = (String name, String typeParameters, String parameters, CodeAnalysis.Library.EquatableList<String> constraints, String arguments);

/// <summary>
/// Generates appendables for opertor-based use of the IndentedStringBuilder.
/// </summary>
[Generator(LanguageNames.CSharp)]
public sealed class IndentedStringBuilderAppendablesGenerator : IIncrementalGenerator
{
    /// <inheritdoc/>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var provider = context.SyntaxProvider.CreateSyntaxProvider(
                Predicate,
                GetSignaturesStep)
            .SelectMany((s, _) => s)
            .Collect()
            .Select((s, _) => s.ToImmutableHashSet())
            .Select(FinalStep);

        context.RegisterSourceOutput(provider, (ctx, source) =>
            ctx.AddSource($"IndentedStringBuilder.Appendables.g.cs", source));
    }

    private const String _backingFieldsTypeName = "BackingFields";
    private static readonly ImmutableArray<Signature> _emptySignaturesArray =
        ImmutableArray.Create<Signature>();
    private static readonly SymbolDisplayFormat _fullyQualifiedFormat =
        SymbolDisplayFormat.FullyQualifiedFormat.WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted);
    private static readonly HashSet<String> _specialConstraints =
        ["class", "struct", "new()", "class?", "notnull", "default", "unmanaged"];
    private static String GetArgumentsText(ParameterListSyntax parameterList)
    {
        var parameters = parameterList.Parameters;
        if(parameters.Count == 0)
            return String.Empty;

        var resultBuilder = new StringBuilder();
        var i = 0;
        append();

        for(i = 1; i < parameters.Count; i++)
        {
            _ = resultBuilder.Append(", ");
            append();
        }

        var result = resultBuilder.ToString();

        return result;

        void append() => resultBuilder.Append(parameters[i].Identifier.Text);
    }

    private static EquatableList<String> GetConstraintsText(
        SyntaxList<TypeParameterConstraintClauseSyntax> clauses,
        SemanticModel semanticModel,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if(clauses.Count == 0)
            return EquatableList<String>.Empty;

        var resultList = new List<String>();
        var constraintBuilder = new StringBuilder();

        for(var i = 0; i < clauses.Count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var clause = clauses[i];

            _ = constraintBuilder.Clear()
                .Append("where ")
                .Append(clause.Name)
                .Append(" : ");

            var constraints = clause.Constraints;
            if(constraints.Count == 0)
                return EquatableList<String>.Empty;

            var j = 0;
            if(!tryAppend())
                return EquatableList<String>.Empty;

            for(j = 1; j < constraints.Count; j++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                _ = constraintBuilder.Append(", ");
                if(!tryAppend())
                    return EquatableList<String>.Empty;
            }

            var constraint = constraintBuilder.ToString();
            resultList.Add(constraint);

            Boolean tryAppend()
            {
                var constraint = constraints[j];

                var text = constraint.ToString();
                if(_specialConstraints.Contains(text))
                {
                    _ = constraintBuilder.Append(text);
                    return true;
                }

                var typeSyntax = SyntaxFactory.ParseTypeName(text);
                var type = semanticModel.GetSpeculativeTypeInfo(
                    constraint.SpanStart,
                    typeSyntax,
                    SpeculativeBindingOption.BindAsTypeOrNamespace).Type; //TODO: is there a different way to obtain a symbol for a constraint?

                if(type == null)
                    return false;

                var fullName = type.ToDisplayString(_fullyQualifiedFormat);
                _ = constraintBuilder.Append(fullName);

                return true;
            }
        }

        var result = resultList.AsEquatable();

        return result;
    }

    private static String GetParametersText(
        ParameterListSyntax parameterList,
        SemanticModel semanticModel,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var enumerator = parameterList.Parameters.GetEnumerator();

        if(!enumerator.MoveNext())
            return String.Empty;

        var resultBuilder = new StringBuilder();

        if(!tryAppend())
            return String.Empty;

        while(enumerator.MoveNext())
        {
            cancellationToken.ThrowIfCancellationRequested();
            _ = resultBuilder.Append(", ");
            if(!tryAppend())
                return String.Empty;
        }

        return resultBuilder.ToString();

        Boolean tryAppend()
        {
            var type = enumerator.Current.Type != null ?
                semanticModel.GetTypeInfo(enumerator.Current.Type, cancellationToken).Type?
                    .ToDisplayString(_fullyQualifiedFormat) ??
                    enumerator.Current.Type?.ToString() :
                null;

            if(type == null)
                return false;

            if(enumerator.Current.Modifiers.Count > 0)
            {
                _ = resultBuilder!.Append(enumerator.Current.Modifiers.ToString())
                    .Append(' ');
            }

            _ = resultBuilder!.Append(type)
                .Append(' ')
                .Append(enumerator.Current.Identifier.Text);

            return true;
        }
    }

    private static Boolean IsTargetSymbol(ISymbol? symbol) =>
        symbol is INamedTypeSymbol &&
        symbol.Name == typeof(IndentedStringBuilder).Name &&
        symbol.ContainingNamespace.ToDisplayString(_fullyQualifiedFormat) == typeof(IndentedStringBuilder).Namespace;

    private static String FinalStep(ImmutableHashSet<Signature> signatures, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var fieldsBuilder = new IndentedStringBuilder(IndentedStringBuilderOptions.Default)
            .Append("file static class StaticAppendableInstances")
            .OpenBracesBlock();

        var builder = new IndentedStringBuilder(IndentedStringBuilderOptions.GeneratedFile with { GeneratorName = "RhoMicro.CodeAnalysis.IndentedStringBuilderAppendablesGenerator" })
            .AppendLine("using RhoMicro.CodeAnalysis.Library.Text;")
            .AppendLine()
            .Append("namespace RhoMicro.CodeAnalysis.Library.Text")
            .OpenBracesBlock()
            .Append("partial class IndentedStringBuilder")
            .OpenBracesBlock()
            .Append("public static partial class Appendables")
            .OpenBracesBlock()
            .AppendLine("public static IndentedStringBuilderAppendable NewLine => AppendLine();");

        foreach(var (name, typeParameters, parameters, constraints, arguments) in signatures)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var fieldName = String.Empty;

            if(parameters == String.Empty && constraints.Count == 0)
            {
                fieldName = $"Appendable_{Guid.NewGuid().ToString().Replace('-', '_')}";
                _ = fieldsBuilder.Append("public static IndentedStringBuilderAppendable ")
                    .Append(fieldName)
                    .Append(" { get; } =");
            }

            _ = builder.Append("public static IndentedStringBuilderAppendable ")
                .Append(name);

            _ = builder.Append(typeParameters)
                .OpenParensBlock()
                .Append(parameters)
                .CloseBlock()
                .Indent();

            if(constraints.Count > 0)
            {
                _ = builder.AppendLine().AppendJoinLines(String.Empty, constraints);
            }

            _ = builder.Append(" =>");

            if(parameters == String.Empty && constraints.Count == 0)
                _ = builder.Append(" StaticAppendableInstances.").Append(fieldName).AppendLine(';');

            _ = (parameters == String.Empty && constraints.Count == 0 ? fieldsBuilder : builder).AppendLine()
                .Append("new")
                .OpenBlock(Blocks.Parens with { ClosingDelimiter = ");\n" })
                    .Append("b => ")
                    .OpenBlock(Blocks.Braces with { ClosingDelimiter = '}' })
                        .Append("b.").Append(name).OpenParensBlock().Append(arguments).CloseBlock().Append(';')
                    .CloseBlock()
                .CloseBlock();

            _ = builder.Detent();
        }

        var fields = fieldsBuilder.CloseAllBlocks().ToString();
        var result = builder.CloseAllBlocks().AppendLine(fields).ToString();

        return result;
    }
    private static ImmutableArray<Signature> GetSignaturesStep(GeneratorSyntaxContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var symbol = context.SemanticModel.GetDeclaredSymbol(context.Node, cancellationToken);
        if(!IsTargetSymbol(symbol))
        {
            return _emptySignaturesArray;
        }

        var cds = (ClassDeclarationSyntax)context.Node;
        var result = cds.Members
            .OfType<MethodDeclarationSyntax>()
            .Where(mds => IsTargetSymbol(context.SemanticModel.GetTypeInfo(mds.ReturnType).Type))
            .Where(mds => mds.Modifiers.Any(m => m.Text == "public") &&
                !mds.Modifiers.Any(m => m.Text == "static"))
            .Select(mds => (
                name: mds.Identifier.Text,
                typeParameters: mds.TypeParameterList?.ToString() ?? String.Empty,
                parameters: GetParametersText(mds.ParameterList, context.SemanticModel, cancellationToken),
                constraints: GetConstraintsText(mds.ConstraintClauses, context.SemanticModel, cancellationToken),
                arguments: GetArgumentsText(mds.ParameterList)))
            .ToImmutableArray();

        return result;
    }
    private static Boolean Predicate(SyntaxNode node, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        if(node is not ClassDeclarationSyntax
            {
                Identifier.Text: "IndentedStringBuilder",
                Keyword.Text: "class",
                Arity: 0
            })
        {
            return false;
        }

        ct.ThrowIfCancellationRequested();
        var tds = (ClassDeclarationSyntax)node;
        if(!tds.Modifiers.Any(m => m.Text == "partial"))
            return false;

        return true;
    }
}
