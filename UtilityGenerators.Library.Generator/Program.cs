﻿using System.Globalization;
using System.Text.RegularExpressions;

var commonPath = args.Length > 0 ? Path.GetFullPath(args[0], Directory.GetCurrentDirectory()) : throw new ArgumentException("Expected valid common path as first arg.", "commonPath");
var generatorPath = args.Length == 2 ? Path.GetFullPath(args[1], Directory.GetCurrentDirectory()) : throw new ArgumentException("Expected generator file path as second arg.", "generatorPath");

var files = Directory.EnumerateFiles(commonPath, "*.cs")
    .Select(p => (SourceText: File.ReadAllText(p), FileName: Path.GetFileName(p), Path: p))
    .Select(t => (
        t.Path,
        t.SourceText,
        RawDollarsCount: RawStringLiteralBraces().Matches(t.SourceText).Select(m => m.Length).Append(1).Max() + 1,
        RawQuotesCount: RawStringLiteralQuotes().Matches(t.SourceText).Select(m => m.Length).Append(2).Max() + 1,
        HintName: t.FileName.Insert(t.FileName.Length - ".cs".Length, ".g")))
    .Select(t => (
        t.Path,
        t.SourceText,
        t.HintName,
        RawDollars: String.Concat(Enumerable.Repeat('$', t.RawDollarsCount)),
        RawBracesOpen: String.Concat(Enumerable.Repeat('{', t.RawDollarsCount)),
        RawBracesClose: String.Concat(Enumerable.Repeat('}', t.RawDollarsCount)),
        RawQuotes: String.Concat(Enumerable.Repeat('"', t.RawQuotesCount))))
    .Select(t => $$"""
            
            #if DEBUG
            path = @"// {{t.Path.Replace("\"", "\\\"")}}";
            #endif
            c.AddSource(
                "{{t.HintName}}", 
            {{t.RawDollars}}{{t.RawQuotes}}
            // <auto-generated>
            // This file was last generated by the RhoMicro.CodeAnalysis.UtilityGeneratorsLibraryGenerator on {{t.RawBracesOpen}}DateTimeOffset.Now{{t.RawBracesClose}}
            // </auto-generated>
            {{t.RawBracesOpen}}path{{t.RawBracesClose}}
            #pragma warning disable
            {{t.SourceText}}
            {{t.RawQuotes}});
            """);
CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
var source = $$"""
    // <auto-generated>
    // This file was last generated by the RhoMicro.CodeAnalysis.UtilityGenerators.Library.Generator on {{DateTimeOffset.UtcNow}}
    // </auto-generated>
    #pragma warning disable

    namespace RhoMicro.CodeAnalysis.UtilityGenerators;

    using Microsoft.CodeAnalysis;
    using System;
        
    /// <summary>
    /// Generates types from the <c>RhoMicro.CodeAnalysis.UtilityGenerators.Library</c> library into the target project.
    /// </summary>
    [Generator(LanguageNames.CSharp)]
    public sealed partial class LibraryGenerator : IIncrementalGenerator
    {
        static partial void InitializeStatic(IncrementalGeneratorInitializationContext context);
        /// <inheritdoc/>
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            InitializeStatic(context);
            context.RegisterPostInitializationOutput(static c =>
            {
                String path = String.Empty;

                {{String.Concat(files)}}
            });
        }
    }
    """;

File.WriteAllText(generatorPath, source);

Console.WriteLine($"Generated files from {commonPath} into generator in {generatorPath}");
partial class Program
{
    [GeneratedRegex("(\"*)")]
    private static partial Regex RawStringLiteralQuotes();
    [GeneratedRegex("({|})*")]
    private static partial Regex RawStringLiteralBraces();
}