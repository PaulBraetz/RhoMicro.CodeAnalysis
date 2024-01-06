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
        var tokenizer = new Tokenizer([]);
        var provider = context.AdditionalTextsProvider
            .Where(text => Path.GetExtension(text.Path) == _grammarFileExtension)
            .Select((text, ct) => tokenizer.Tokenize(text, ct))
            .WithComparer(CollectionEqualityComparer<Token>.Instance)
            .Select((tokens, ct) => String.Concat(tokens))
            .Collect()
            .WithComparer(CollectionEqualityComparer<String>.Instance)
            .Select((tokens, ct) => $"/*\n{String.Join("\n\nNext Grammar\n\n", tokens)}\n*/");

        context.RegisterSourceOutput(provider, (ctx, tokens) => ctx.AddSource("Tokens.txt", tokens));
        IncludedFileSources.RegisterToContext(context);
    }
}
