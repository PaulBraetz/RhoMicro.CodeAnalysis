namespace RhoMicro.CodeAnalysis.DocReflect.Generators;

using RhoMicro.CodeAnalysis.Library.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System.Collections.Immutable;
using System.Xml;

using static RhoMicro.CodeAnalysis.Library.Text.IndentedStringBuilder.Appendables;

/// <summary>
/// Generates documentation providers for DocReflect.
/// </summary>
[Generator(LanguageNames.CSharp)]
public sealed class DocReflect : IIncrementalGenerator
{
    /// <inheritdoc/>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var provider = context.SyntaxProvider.CreateSyntaxProvider(
            (node, ct) => node is TypeDeclarationSyntax,
            ExtractCommentStep)
            .Select(ParseStep)
            .Collect()
            .Select(FinalStep);
    }

    static ParseStepResult ParseStep(ExtractCommentStepResult result, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
    static FinalStepResult FinalStep(ImmutableArray<ParseStepResult> results, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var options = IndentedStringBuilderOptions.GeneratedFile with
        {
            AmbientCancellationToken = cancellationToken,
            GeneratorName = "RhoMicro.CodeAnalysis.DocReflect"
        };

        var builder = new IndentedStringBuilder(options)
            .Append("namespace RhoMicro.CodeAnalysis.Generated.DocReflect")
            .OpenBracesBlock()
            .Append("internal sealed class TypeDocumentationProvider : RhoMicro.CodeAnalysis.DocReflect.Infrastructure.IDocumentationProvider")
            .OpenBracesBlock()
            .AppendLine("/// <inheritdoc/>")
            .AppendLine("public IEnumerable<KeyValuePair<MethodInfo, MethodDocumentation>> GetMethodDocumentations() => Array.Empty<KeyValuePair<MethodInfo, MethodDocumentation>>();")
            .AppendLine("/// <inheritdoc/>")
            .AppendLine("public IEnumerable<KeyValuePair<PropertyInfo, PropertyDocumentation>> GetMethodDocumentations() => Array.Empty<KeyValuePair<PropertyInfo, PropertyDocumentation>>();")
            .AppendLine("/// <inheritdoc/>")
            .AppendLine("public IEnumerable<KeyValuePair<Type, TypeDocumentation>> GetTypeDocumentations() =>")
            .OpenBlock(Blocks.Brackets with
            {
                PlaceDelimitersOnNewLine = true,
                Indentation = options.DefaultIndentation
            });

        var sourceText = builder.CloseAllBlocks().ToString();
        const String hintName = "TypeDocumentationProvider.g.cs";
        var result = new FinalStepResult(hintName, sourceText);

        return result;
    }
    static ExtractCommentStepResult ExtractCommentStep(GeneratorSyntaxContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var targetSymbol = context.SemanticModel.GetSymbolInfo(context.Node, cancellationToken).Symbol;
        if(targetSymbol == null)
            return default;

        cancellationToken.ThrowIfCancellationRequested();
        var rawComment = targetSymbol.GetDocumentationCommentXml(expandIncludes: true, cancellationToken: cancellationToken);
        if(rawComment == null)
            return default;

        cancellationToken.ThrowIfCancellationRequested();

        throw new NotImplementedException("Comment parsing is not implemented yet.");
        var parsedComment = new XmlDocument();

        cancellationToken.ThrowIfCancellationRequested();
        var typeMetadataName = targetSymbol.MetadataName;
        var result = new ExtractCommentStepResult(typeMetadataName, rawComment, parsedComment);

        return result;
    }
}
