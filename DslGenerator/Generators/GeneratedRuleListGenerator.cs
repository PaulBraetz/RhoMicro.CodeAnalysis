namespace RhoMicro.CodeAnalysis.DslGenerator.Generators;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using RhoMicro.CodeAnalysis.DslGenerator.Grammar;
using RhoMicro.CodeAnalysis.DslGenerator.Lexing;
using RhoMicro.CodeAnalysis.DslGenerator.Parsing;
using RhoMicro.CodeAnalysis.UtilityGenerators;

using System.Collections.Immutable;
using System.Text;

using InitialStepResult = (String unparsedList, String implTypeName, String partialTypeSourceText);
using TokenizeStepResult = (Lexing.TokenizeResult tokenizeResult, String implTypeName, String partialTypeSourceText);
using ParseStepResult = (Parsing.ParseResult parseResult, String implTypeName, String partialTypeSourceText);
using ImplTypeStepResult = (String implTypeSourceText, String partialTypeSourceText, RhoMicro.CodeAnalysis.DslGenerator.Analysis.DiagnosticsCollection diagnostics);
using FinalStepResult = (String source, RhoMicro.CodeAnalysis.DslGenerator.Analysis.DiagnosticsCollection diagnostics);
using RhoMicro.CodeAnalysis.DslGenerator.Analysis;

[Generator(LanguageNames.CSharp)]
internal class GeneratedRuleListGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var provider = context.SyntaxProvider.ForGeneratedRuleListAttribute(
            IsGeneratorTarget,
            InitialStep)
            .Where(t => t != default)
            .Select(TokenizeStep)
            .Select(ParseStep)
            .Select(ImplTypeStep)
            .WithComparer(_implTypeStepResultComparer)
            .Collect()
            .WithComparer(_implTypeStepResultArrayComparer)
            .Select(FinalStep)
            .WithComparer(_finalStepResultComparer);

        context.RegisterSourceOutput(provider, (ctx, result) =>
        {
            var (source, diagnostics) = result;
            ctx.AddSource("GeneratedRuleLists.g.cs", source);
            diagnostics.ReportToContext(ctx); //TODO: map to attribute span
        });
        context.RegisterPostInitializationOutput(ctx =>
            ctx.AddSource($"{nameof(GeneratedRuleListAttribute)}.g.cs", GeneratedRuleListAttribute.SourceText));
    }

    static readonly EqualityComparerStrategy<ImplTypeStepResult> _implTypeStepResultComparer = new(
            (x, y) => x.implTypeSourceText == y.implTypeSourceText &&
                   x.partialTypeSourceText == y.partialTypeSourceText,
            obj => (obj.implTypeSourceText, obj.partialTypeSourceText).GetHashCode());
    static readonly ImmutableArrayCollectionEqualityComparer<ImplTypeStepResult> _implTypeStepResultArrayComparer =
        new(_implTypeStepResultComparer);
    static readonly EqualityComparerStrategy<FinalStepResult> _finalStepResultComparer = new(
            (x, y) => x.source == y.source,
            obj => obj.source.GetHashCode());
    static readonly SymbolDisplayFormat _typeSignatureNameFormat =
        SymbolDisplayFormat.MinimallyQualifiedFormat.WithGenericsOptions(SymbolDisplayGenericsOptions.IncludeTypeParameters);
    static readonly SymbolDisplayFormat _namespaceFormat =
        SymbolDisplayFormat.FullyQualifiedFormat.WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted);

    static Boolean IsGeneratorTarget(Microsoft.CodeAnalysis.SyntaxNode node, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if(node is not MethodDeclarationSyntax mds || mds.ParameterList.Parameters.Count != 0)
            return false;

        cancellationToken.ThrowIfCancellationRequested();

        var isStatic = false;
        var isPartial = false;
        for(var i = 0; i < mds.Modifiers.Count; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if(mds.Modifiers[i].IsKind(SyntaxKind.StaticKeyword))
            {
                isStatic = true;
                if(isPartial)
                    break;
            } else if(mds.Modifiers[i].IsKind(SyntaxKind.PartialKeyword))
            {
                isPartial = true;
                if(isStatic)
                    break;
            }
        }

        if(!(isStatic || isPartial))
            return false;

        cancellationToken.ThrowIfCancellationRequested();

        var result = mds.Body == null;

        return result;
    }
    static InitialStepResult InitialStep(GeneratorAttributeSyntaxContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var target = (IMethodSymbol)context.TargetSymbol;

        if(target.ReturnType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) !=
            "global::RhoMicro.CodeAnalysis.DslGenerator.Grammar.RuleList")
        {
            return default;
        }

        if(context.Attributes[0].ConstructorArguments[0].Value is not String unparsedList)
            return default;

        var implTypeName = GetImplTypeName(cancellationToken);
        var partialTypeSourceText = GetPartialTypeSourceText(target, implTypeName, cancellationToken);

        var result = (unparsedList, implTypeName, partialTypeSourceText);

        return result;
    }
    static TokenizeStepResult TokenizeStep(InitialStepResult previous, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var tokenizeResult = Tokenizer.Instance.Tokenize(previous.unparsedList, cancellationToken);

        return (tokenizeResult, previous.implTypeName, previous.partialTypeSourceText);
    }
    static ParseStepResult ParseStep(TokenizeStepResult previous, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var parseResult = Parser.Instance.Parse(previous.tokenizeResult, cancellationToken);

        return (parseResult, previous.implTypeName, previous.partialTypeSourceText);
    }
    static ImplTypeStepResult ImplTypeStep(ParseStepResult previous, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var implTypeSourceText = new StringBuilder("file static class ").AppendLine(previous.implTypeName)
            .AppendLine("{")
            .AppendLine("    public static global::RhoMicro.CodeAnalysis.DslGenerator.Grammar.RuleList Instance { get; } =")
            .Append("        ").AppendMetaString(previous.parseResult.RuleList).AppendLine(";")
            .Append('}')
            .ToString();

        return (implTypeSourceText, previous.partialTypeSourceText, previous.parseResult.Diagnostics);
    }
    static FinalStepResult FinalStep(ImmutableArray<ImplTypeStepResult> results, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var resultBuilder = new StringBuilder();
        var diagnosticsAccumulator = new DiagnosticsCollection();
        _ = resultBuilder.AppendLine("// <auto-generated>")
            .Append("// This file was last generated by the RhoMicro.CodeAnalysis.DslGenerator on ").AppendLine(DateTimeOffset.Now.ToString())
            .AppendLine("// </auto-generated>")
            .AppendLine("#pragma warning disable");

        for(var i = 0; i < results.Length; i++)
        {
            var (implTypeSourceText, partialTypeSourceText, diagnostics) = results[i];
            _ = resultBuilder.AppendLine(implTypeSourceText).AppendLine(partialTypeSourceText);
            diagnosticsAccumulator.Add(diagnostics);
        }

        var sourceText = resultBuilder.ToString();

        return (sourceText, diagnosticsAccumulator);
    }

    static String GetPartialTypeSourceText(IMethodSymbol target, String implTypeName, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var builder = new StringBuilder();
        var containingType = target.ContainingType;

        AppendHead(containingType, builder, cancellationToken);
        AppendMethodImplementation(target, implTypeName, builder, cancellationToken);
        AppendTail(builder, containingType, cancellationToken);

        //result
        cancellationToken.ThrowIfCancellationRequested();

        var result = builder.ToString();
        return result;
    }
    static void AppendMethodImplementation(IMethodSymbol target, String implTypeName, StringBuilder builder, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        _ = builder.Append("   ")
            .Append(SyntaxFacts.GetText(target.DeclaredAccessibility))
            .Append(" static partial global::RhoMicro.CodeAnalysis.DslGenerator.Grammar.RuleList ")
            .Append(target.Name)
            .Append("() => ")
            .Append(implTypeName)
            .AppendLine(".Instance;");
    }
    static void AppendTail(StringBuilder builder, INamedTypeSymbol containingType, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var parent = containingType;
        while(parent != null)
        {
            cancellationToken.ThrowIfCancellationRequested();

            _ = builder.AppendLine("}");
            parent = parent.ContainingType;
        }

        if(!containingType.ContainingNamespace.IsGlobalNamespace)
        {
            _ = builder.AppendLine("}");
        }
    }
    static void AppendHead(INamedTypeSymbol parent, StringBuilder builder, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if(parent.ContainingType != null)
        {
            AppendHead(parent.ContainingType, builder, cancellationToken);
        } else if(!parent.ContainingNamespace.IsGlobalNamespace)
        {
            _ = builder
                .Append("namespace ")
                .AppendLine(parent.ContainingNamespace.ToDisplayString(_namespaceFormat))
                .AppendLine("{");
        }

        var typeModifier = parent.IsRecord ?
            parent.IsReferenceType ?
            "record" :
            "record struct " :
            parent.IsReferenceType ?
            "class " :
            "struct ";

        _ = builder
            .Append("partial ")
            .Append(typeModifier)
            .AppendLine(parent.ToDisplayString(_typeSignatureNameFormat))
            .AppendLine("{");
    }
    static String GetImplTypeName(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var id = Guid.NewGuid().ToString().Replace('-', '_');
        var result = $"GeneratedRueList_{id}";

        return result;
    }
}
