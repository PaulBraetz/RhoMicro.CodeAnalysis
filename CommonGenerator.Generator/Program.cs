﻿using System.Globalization;

var commonPath = args.Length > 0 ? Path.GetFullPath(args[0], Directory.GetCurrentDirectory()) : throw new ArgumentException("Expected valid common path as first arg.", "commonPath");
var generatorPath = args.Length == 2 ? Path.GetFullPath(args[1], Directory.GetCurrentDirectory()) : throw new ArgumentException("Expected generator file path as second arg.", "generatorPath");

var files = Directory.EnumerateFiles(commonPath, "*.cs")
    .Select(p => (SourceText: File.ReadAllText(p), FileName: Path.GetFileName(p)))
    .Select(t => (SourceText: t.SourceText.Replace("\"\"\"", "\"\"\"\""), HintName: t.FileName.Insert(t.FileName.Length - ".cs".Length, ".g")))
    .Select(t => $$""""
            c.AddSource(
                "{{t.HintName}}", 
            """
            // <auto-generated>
            // This file was last generated by the RhoMicro.CodeAnalysis.CommonGenerator on {{DateTimeOffset.Now}}
            // </auto-generated>
            #pragma warning disable
            {{t.SourceText}}
            """);

            """");
CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
var source = $$"""
    // <auto-generated>
    // This file was last generated by the RhoMicro.CodeAnalysis.CommonGenerator.Generator on {{DateTimeOffset.UtcNow}}
    // </auto-generated>
    #pragma warning disable

    namespace RhoMicro.CodeAnalysis;

    using Microsoft.CodeAnalysis;
        
    /// <summary>
    /// Generates types from the <c>RhoMicro.CodeAnalysis.Common</c> library into the target project.
    /// </summary>
    [Generator(LanguageNames.CSharp)]
    public sealed class CommonGenerator : IIncrementalGenerator
    {
        /// <inheritdoc/>
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            context.RegisterPostInitializationOutput(static c =>
            {
                {{String.Concat(files)}}
            });
        }
    }
    """;

File.WriteAllText(generatorPath, source);

Console.WriteLine($"Generated files from {commonPath} into generator in {generatorPath}");
