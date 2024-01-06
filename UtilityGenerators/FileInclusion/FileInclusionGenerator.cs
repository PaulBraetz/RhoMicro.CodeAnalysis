﻿namespace RhoMicro.CodeAnalysis.UtilityGenerators;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

/// <summary>
/// Generates static source texts from annotated source files for inclusion in both a generator as well as its target compilation.
/// </summary>
[Generator(LanguageNames.CSharp)]
public sealed class FileInclusionGenerator : IIncrementalGenerator
{
    const String _hintName = "IncludedFiles.g.cs";
    const String _attributeMetadataName = "RhoMicro.CodeAnalysis.IncludeFileAttribute";
    const String _attributeHintName = "IncludeFileAttribute.g.cs";
    const String _attributeSource =
        """
        using System;
        namespace RhoMicro.CodeAnalysis
        {
            [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface | AttributeTargets.Delegate, AllowMultiple = false, Inherited = false)]
            internal sealed class IncludeFileAttribute : Attribute
            { }
        }
        """;
    static readonly Regex _rawStringLiteralBraces = new("({|})*", RegexOptions.Compiled);
    static readonly Regex _rawStringLiteralQuotes = new("(\"*)", RegexOptions.Compiled);

    /// <inheritdoc/>
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var sourceProvider = context.SyntaxProvider.ForAttributeWithMetadataName(
            _attributeMetadataName,
            (node, ct) => true,
            (ctx, ct) => (sourcetext: ctx.TargetNode.SyntaxTree.ToString(), path: ctx.TargetNode.SyntaxTree.FilePath))
            .Select((t, ct) =>
            {
                var (sourceText, path) = t;

                var hintName = CreateHintName(path);

                var result = (hintName, sourceText);

                return result;
            })
            .Select((t, ct) =>
            {
                var (hintName, sourceText) = t;
                var rawDollarsCount = _rawStringLiteralBraces.Matches(sourceText)
                    .Cast<Match>()
                    .Select(m => m.Length)
                    .Append(1)
                    .Max() + 1;
                var rawQuotesCount = _rawStringLiteralQuotes.Matches(sourceText)
                    .Cast<Match>()
                    .Select(m => m.Length)
                    .Append(2)
                    .Max() + 1;

                return (rawDollarsCount, rawQuotesCount, sourceText, hintName);
            })
            .Select((t, ct) =>
            {
                var (rawDollarsCount, rawQuotesCount, sourceText, hintName) = t;

                var rawDollars = StringRepeat('$', rawDollarsCount);
                var rawBracesOpen = StringRepeat('{', rawDollarsCount);
                var rawBracesClose = StringRepeat('}', rawDollarsCount);
                var rawQuotes = StringRepeat('"', rawQuotesCount);

                var addSourceStatement =
                $$"""
                c.AddSource(
                    "{{hintName}}",
                {{rawDollars}}{{rawQuotes}}
                // <auto-generated>
                // This file was last generated by the RhoMicro.CodeAnalysis.FileInclusionGenerator on {{rawBracesOpen}}now{{rawBracesClose}}
                // </auto-generated>
                #pragma warning disable
                {{sourceText}}
                {{rawQuotes}});
                """;
                return addSourceStatement;
            })
            .Collect()
            .WithComparer(ImmutableArrayCollectionEqualityComparer<String>.Default)
            .Select((addSourceStatements, ct) =>
            {
                var builder = new StringBuilder(
                    $$"""
                    // <auto-generated>
                    // This file was last generated by the RhoMicro.CodeAnalysis.FileInclusionGenerator on {{DateTimeOffset.Now}}
                    // </auto-generated>
                    using Microsoft.CodeAnalysis;
                    using System;

                    namespace RhoMicro.CodeAnalysis.Generated
                    {
                        internal static class IncludedFileSources
                        {
                            public static void RegisterToContext(IncrementalGeneratorInitializationContext context)
                            {
                    """);

                if(addSourceStatements.Length > 0)
                {
                    _ = builder.Append(
                    """
                    
                                context.RegisterPostInitializationOutput(c =>
                                {
                                    var now = DateTimeOffset.Now;
                    """);
                }

                foreach(var addSourceStatement in addSourceStatements)
                {
                    _ = builder.AppendLine(addSourceStatement);
                }

                if(addSourceStatements.Length > 0)
                {
                    _ = builder.Append(
                    """
                                });
                    """);
                }

                var result = builder.Append(
                    """
                            }
                        }
                    }
                    """).ToString();

                return result;
            });

        context.RegisterSourceOutput(sourceProvider, (ctx, sourceText) => ctx.AddSource(_hintName, sourceText));
        context.RegisterPostInitializationOutput(c => c.AddSource(_attributeHintName, _attributeSource));
    }

    static String CreateHintName(String path) => $"{Path.GetFileNameWithoutExtension(path)}_{Guid.NewGuid()}.g.cs";
    static String StringRepeat(Char c, Int32 count) => String.Concat(Enumerable.Repeat(c, count));
}
