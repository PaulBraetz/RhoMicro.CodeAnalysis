namespace RhoMicro.CodeAnalysis.UtilityGenerators.Library.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using RhoMicro.CodeAnalysis.Library.Text;

using System;
using System.Collections.Immutable;
using System.Text;
using System.Threading;

using Signature = (String name, String typeParameters, String parameters, String constraints, String arguments, String backingFieldName);

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
            .WithComparer(ImmutableArrayCollectionEqualityComparer<Signature>.Default)
            .Where(a => a.Length > 0)
            .Select(FinalStep);

        context.RegisterSourceOutput(provider, (ctx, source) =>
            ctx.AddSource($"IndentedStringBuilder.Appendables_{Guid.NewGuid().ToString().Replace('-', '_')}.g.cs", source));
    }

    private const String _backingFieldsTypeName = "BackingFields";
    private static readonly ImmutableArray<Signature> _emptySignaturesArray =
        ImmutableArray.Create<Signature>();
    private static readonly SymbolDisplayFormat _fullyQualifiedFormat =
        SymbolDisplayFormat.FullyQualifiedFormat.WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted);
    private static readonly HashSet<String> _specialConstraints =
        ["class", "struct", "new()", "class?", "notnull", "default", "unmanaged"];
    private static String GetArgumentsText(ParameterListSyntax parameterList) =>
        parameterList.Parameters.Count == 0 ?
        String.Empty :
        String.Join(", ", parameterList.Parameters.Select(p => p.Identifier.Text));
    private static String GetConstraintsText(
        SyntaxList<TypeParameterConstraintClauseSyntax> clauses,
        SemanticModel semanticModel,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if(clauses.Count == 0)
            return String.Empty;

        var resultBuilder = new StringBuilder();

        for(var i = 0; i < clauses.Count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var clause = clauses[i];

            _ = resultBuilder.AppendLine().Append("where ")
                .Append(clause.Name)
                .Append(" : ");

            var constraints = clause.Constraints;
            if(constraints.Count == 0)
                return String.Empty;

            var j = 0;
            if(!tryAppend())
                return String.Empty;

            for(j = 1; j < constraints.Count; j++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                _ = resultBuilder.Append(", ");
                if(!tryAppend())
                    return String.Empty;
            }

            Boolean tryAppend()
            {
                var constraint = constraints[j];

                var text = constraint.ToString();
                if(_specialConstraints.Contains(text))
                {
                    _ = resultBuilder.Append(text);
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
                _ = resultBuilder.Append(fullName);

                return true;
            }
        }

        var result = resultBuilder.ToString();

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

    private static String FinalStep(ImmutableArray<Signature> signatures, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var builder = new IndentedStringBuilder(IndentedStringBuilderOptions.GeneratedFile)
            .Append("namespace RhoMicro.CodeAnalysis.Library.Text")
            .OpenBracesBlock()
            .Append("partial class IndentedStringBuilder")
            .OpenBracesBlock()
            .Append("public static partial class Appendables")
            .OpenBracesBlock();

        for(var i = 0; i < signatures.Length; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var (name, typeParameters, parameters, constraints, arguments, backingFieldName) = signatures[i];
            var hasParameters = parameters != String.Empty;

            var generatedName = name.StartsWith("Append") ?
                name[6..] :
                name;

            _ = builder.Append("public static IndentedStringBuilderAppendable ")
                .Append(generatedName);

            _ = builder.Append(typeParameters)
                .OpenParensBlock()
                .Append(parameters)
                .CloseBlock()
                .Append(constraints)
                .Append(" =>")
                .AppendLine()
                .Indent();
            _ = (hasParameters ?
                builder
                .Append("new")
                 .OpenBlock(Blocks.Parens with { ClosingDelimiter = ");\n" })
                     .Append("(b, c) => ")
                     .OpenBlock(Blocks.Braces with { ClosingDelimiter = '}' })
                         .AppendLine("c.ThrowIfCancellationRequested();")
                         .Append("b.").Append(name).OpenParensBlock().Append(arguments).CloseBlock().Append(';')
                     .CloseBlock()
                 .CloseBlock() :
                 builder.Append(_backingFieldsTypeName).Append('.').Append(backingFieldName).Append(';'))
                 .Detent();
        }

        var result = builder.CloseAllBlocks().ToString();

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
            .Where(mds => mds.Identifier.Text != "Append" &&
                mds.Modifiers.Any(m => m.Text == "public") &&
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
