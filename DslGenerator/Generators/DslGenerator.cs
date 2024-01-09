namespace RhoMicro.CodeAnalysis.DslGenerator.Generators;

using Microsoft.CodeAnalysis;
using RhoMicro.CodeAnalysis.Generated;
using RhoMicro.CodeAnalysis.DslGenerator.Lexing;

/// <summary>
/// Generates utilities for domain specific languages.
/// </summary>
[Generator(LanguageNames.CSharp)]
public sealed class DslGenerator : IIncrementalGenerator
{
    const String _grammarFileExtension = ".rmbnf";
    /// <inheritdoc/>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var provider = context.AdditionalTextsProvider
            .Where(text => Path.GetExtension(text.Path) == _grammarFileExtension)
            .Select((text, ct) => new Tokenizer().Tokenize(text, ct))
            /*.Select((tokenizeResult, ct) => (tokenizeResult, new Parser().Parse(tokenizeResult.Tokens, ct)))*/;

        context.RegisterSourceOutput(provider, (ctx, tokenizationResult) =>
        {
            var (tokens, diagnostics) = tokenizationResult;
            ctx.AddSource("Tokens.g.cs", $"//{String.Concat(tokens)}");
            diagnostics.ReportToContext(ctx);
        });
        IncludedFileSources.RegisterToContext(context);
    }
}
