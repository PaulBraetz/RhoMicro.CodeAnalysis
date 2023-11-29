namespace RhoMicro.CodeAnalysis.AttributeFactoryGenerator;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

public sealed partial class AttributeFactoryGenerator
{
    readonly struct AttributeSourceModel(String source, ClassDeclarationSyntax declaration, SemanticModel semanticModel, INamedTypeSymbol symbol) : IEquatable<AttributeSourceModel>
    {
        public readonly String Source = source;
        public readonly ClassDeclarationSyntax Declaration = declaration;
        public readonly SemanticModel SemanticModel = semanticModel;
        public readonly INamedTypeSymbol Symbol = symbol;

        public AttributeSourceModel WithSource(String source) => new(source, Declaration, SemanticModel, Symbol);

        public static AttributeSourceModel Create(ClassDeclarationSyntax declaration, SemanticModel semanticModel, CancellationToken token)
        {
            var symbol = semanticModel.GetDeclaredSymbol(declaration, cancellationToken: token) ??
                throw new ArgumentException("The symbol of the declaration passed could not be determined using the semantic model provided.", nameof(declaration));

            using var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            declaration.SyntaxTree.GetText(token).Write(writer, token);
            writer.Flush();
            stream.Position = 0;
            var reader = new StreamReader(stream);
            var sourceText = reader.ReadToEnd();
            var quotedSourceText = GetRawSourceTextLiteral(sourceText);

            var source = _sourceTemplate.Replace(_sourceTextPlaceholder, quotedSourceText);

            var result = new AttributeSourceModel(source, declaration, semanticModel, symbol);

            return result;
        }

        private static readonly Regex _rawStringLiteralPattern = new("\"*", RegexOptions.Compiled);

        private static String GetRawSourceTextLiteral(String sourceText)
        {
            var longestMatch = _rawStringLiteralPattern.Matches(sourceText)
                .OfType<Match>()
                .Select(m => m.Length)
                .Max();

            var quoteCount = longestMatch > 2 ?
                longestMatch + 1 :
                3;
            var quotes = String.Concat(Enumerable.Repeat('"', quoteCount));
            var result = $"{quotes}\n{sourceText}\n{quotes}";

            return result;
        }

        public override Boolean Equals(Object obj) => obj is AttributeSourceModel model && Equals(model);
        public Boolean Equals(AttributeSourceModel other) => Source == other.Source;
        public override Int32 GetHashCode() => 924162744 + EqualityComparer<String>.Default.GetHashCode(Source);

        public static Boolean operator ==(AttributeSourceModel left, AttributeSourceModel right) => left.Equals(right);
        public static Boolean operator !=(AttributeSourceModel left, AttributeSourceModel right) => !(left == right);
    }
}
