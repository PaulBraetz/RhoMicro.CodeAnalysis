namespace RhoMicro.CodeAnalysis.Library;

using Microsoft.CodeAnalysis.CSharp;

using System;
using System.Collections.Generic;

/// <summary>
/// Represents the settings used for formatting the c# output produced by an instance of <see cref="IExpandingMacroStringBuilder{TMacro}"/>.
/// </summary>
/// <param name="parseOptions">
/// The options to use when parsing the generated source text.
/// </param>
/// <param name="indentation">
/// An optional sequence of whitespace characters that defines a single level of
/// indentation.
/// </param>
/// <param name="endOfLine">
/// An optional sequence of whitespace characters used for end of line.
/// </param>
/// <param name="elasticTrivia">
/// If true the replaced trivia is elastic trivia.
/// </param>
readonly struct CSharpBuilderFormattingOptions(
    CSharpParseOptions parseOptions,
    String indentation = "\t",
    String endOfLine = "\r\n",
    Boolean elasticTrivia = false) : IEquatable<CSharpBuilderFormattingOptions>
{
    /// <summary>
    /// The default options.
    /// </summary>
    public static readonly CSharpBuilderFormattingOptions Default = new(CSharpParseOptions.Default);

    /// <summary>
    /// The options to use when parsing the generated source text.
    /// </summary>
    public CSharpParseOptions ParseOptions { get; } = parseOptions;
    /// <summary>
    /// An optional sequence of whitespace characters that defines a single level of
    /// indentation.
    /// </summary>
    public String Indentation { get; } = indentation;
    /// <summary>
    /// An optional sequence of whitespace characters used for end of line.
    /// </summary>
    public String EndOfLine { get; } = endOfLine;
    /// <summary>
    /// If true the replaced trivia is elastic trivia.
    /// </summary>
    public Boolean ElasticTrivia { get; } = elasticTrivia;

    /// <inheritdoc/>
    public override Boolean Equals(Object? obj) => obj is CSharpBuilderFormattingOptions options && Equals(options);
    /// <inheritdoc/>
    public Boolean Equals(CSharpBuilderFormattingOptions other) => EqualityComparer<CSharpParseOptions>.Default.Equals(ParseOptions, other.ParseOptions)
        && Indentation == other.Indentation
        && EndOfLine == other.EndOfLine
        && ElasticTrivia == other.ElasticTrivia;
    /// <inheritdoc/>
    public override Int32 GetHashCode()
    {
        var hashCode = -551825062;
        hashCode = hashCode * -1521134295 + EqualityComparer<CSharpParseOptions>.Default.GetHashCode(ParseOptions);
        hashCode = hashCode * -1521134295 + EqualityComparer<String>.Default.GetHashCode(Indentation);
        hashCode = hashCode * -1521134295 + EqualityComparer<String>.Default.GetHashCode(EndOfLine);
        hashCode = hashCode * -1521134295 + ElasticTrivia.GetHashCode();
        return hashCode;
    }
    public static Boolean operator ==(CSharpBuilderFormattingOptions left, CSharpBuilderFormattingOptions right) => left.Equals(right);
    public static Boolean operator !=(CSharpBuilderFormattingOptions left, CSharpBuilderFormattingOptions right) => !(left == right);
}
