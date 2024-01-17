namespace RhoMicro.CodeAnalysis.DslGenerator.Generators;

using Microsoft.CodeAnalysis;
using RhoMicro.CodeAnalysis.Generated;
using RhoMicro.CodeAnalysis.Library;
using RhoMicro.CodeAnalysis.DslGenerator.Lexing;
using RhoMicro.CodeAnalysis.DslGenerator.Parsing;
using RhoMicro.CodeAnalysis.DslGenerator.Grammar;
using RhoMicro.CodeAnalysis.DslGenerator.Analysis;
using System.Collections.Immutable;
using RhoMicro.CodeAnalysis.Library.Text;

/// <summary>
/// Generates utilities for domain specific languages.
/// </summary>
[Generator(LanguageNames.CSharp)]
public sealed class DslGenerator : IIncrementalGenerator
{
    const String _grammarFileExtension = ".rmbnf";
    static readonly IEqualityComparer<(String source, DiagnosticsCollection diagnostics)> _sourceDiagnosticsTupleComparer =
        new EqualityComparerStrategy<(String source, DiagnosticsCollection diagnostics)>(
                (x, y) => x.source == y.source,
                obj => obj.source.GetHashCode());
    static readonly IEqualityComparer<ImmutableArray<(String source, DiagnosticsCollection diagnostics)>> _sourceDiagnosticsTupleArrayComparer =
        new ImmutableArrayCollectionEqualityComparer<(String source, DiagnosticsCollection diagnostics)>(_sourceDiagnosticsTupleComparer);

    /// <inheritdoc/>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var provider = context.AdditionalTextsProvider
            .Where(text => Path.GetExtension(text.Path) == _grammarFileExtension)
            .Select((text, ct) => (tokenizeResult: new Tokenizer().Tokenize(text.GetText(ct)?.ToString() ?? String.Empty, ct, text.Path), fileName: Path.GetFileNameWithoutExtension(text.Path)))
            .Select((t, ct) => (parseResult: new Parser().Parse(t.tokenizeResult, ct), t.fileName))
            .Select((t, ct) => (t.parseResult, isNamed: t.parseResult.RuleList is NamedRuleList, t.fileName))
            .Select((t, ct) => (
                t.parseResult,
                name: t.isNamed ? ((NamedRuleList)t.parseResult.RuleList).Name.ToDisplayString(ct) : t.fileName,
                type: t.isNamed ? nameof(NamedRuleList) : nameof(RuleList)))
            .Where(t => Microsoft.CodeAnalysis.CSharp.SyntaxFacts.IsValidIdentifier(t.name))
            .Select((t, ct) =>
            {
                ct.ThrowIfCancellationRequested();

                var (parseResult, name, type) = t;
                var (ruleList, diagnostics) = parseResult;
                var source = new IndentedStringBuilder()
                    .Indent()
                    .Comment.OpenSummary()
                    .Comment.OpenParagraph()
                    .Append("Matches the following grammar:")
                    .CloseBlock() //<para/>
                    .AppendCommentParagraphs(ruleList, ct)
                    .CloseBlock() //<summary/>
                    .Append("public static ").Append(type).Append(' ').Append(name).Append(' ').AppendLine("{ get; } = ")
                    .OpenIndentBlock()
                    .AppendMetaString(ruleList, ct).AppendLine(";")
                    .CloseBlock()
                    .ToString();

                return (source, diagnostics);
            })
            .WithComparer(_sourceDiagnosticsTupleComparer)
            .Collect()
            .WithComparer(_sourceDiagnosticsTupleArrayComparer)
            .Select((sourceTuples, ct) =>
            {
                var diagnosticsAggregator = new DiagnosticsCollection();
                var sourceBuilder = new IndentedStringBuilder(
                    IndentedStringBuilderOptions.GeneratedFile with
                    {
                        GeneratorName = "RhoMicro.CodeAnalysis.DslGenerator"
                    })
                    .AppendLine("namespace RhoMicro.CodeAnalysis.DslGenerator.Grammar.Generated;")
                    .Append("static class GeneratedRuleLists")
                    .OpenBracesBlock();

                for(var i = 0; i < sourceTuples.Length; i++)
                {
                    var (source, diagnostics) = sourceTuples[i];
                    _ = sourceBuilder.AppendLine(source);
                    diagnosticsAggregator.Add(diagnostics);
                }

                var ruleListsSource = sourceBuilder.CloseAllBlocks().ToString();

                return (source: ruleListsSource, diagnostics: diagnosticsAggregator);
            })
            .WithComparer(new EqualityComparerStrategy<(String source, DiagnosticsCollection diagnostics)>(
                (x, y) => x.source == y.source,
                obj => obj.source.GetHashCode()));

        context.RegisterSourceOutput(provider, (ctx, data) =>
        {
            var (source, diagnostics) = data;
            ctx.AddSource("GeneratedRuleLists.g.cs", source);
            diagnostics.ReportToContext(ctx);
        });
        IncludedFileSources.RegisterToContext(context);
    }
}
