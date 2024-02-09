#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
namespace RhoMicro.CodeAnalysis.UtilityGenerators;

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
    readonly struct AttributeSourceModel(String source, TypeDeclarationSyntax declaration, SemanticModel semanticModel, INamedTypeSymbol symbol) : IEquatable<AttributeSourceModel>
    {
        public readonly String Source = source;
        public readonly TypeDeclarationSyntax Declaration = declaration;
        public readonly SemanticModel SemanticModel = semanticModel;
        public readonly INamedTypeSymbol Symbol = symbol;

        public AttributeSourceModel WithSource(String source) => new(source, Declaration, SemanticModel, Symbol);

        public static AttributeSourceModel Create(GeneratorAttributeSyntaxContext context, CancellationToken token)
        {
            var declaration = (TypeDeclarationSyntax)context.TargetNode;
            var semanticModel = context.SemanticModel;

            var symbol = semanticModel.GetDeclaredSymbol(declaration, cancellationToken: token) ??
                throw new ArgumentException("The symbol of the declaration passed could not be determined using the semantic model provided.", nameof(declaration));

            using var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            declaration.SyntaxTree.GetText(token).Write(writer, token);
            writer.Flush();
            stream.Position = 0;
            var reader = new StreamReader(stream);
            var sourceText = reader.ReadToEnd();
            var quotedSourceText = GetRawSourceTextValue(sourceText);

            var targetSymbol = (INamedTypeSymbol)context.TargetSymbol;
            var declarationKeyword = targetSymbol switch
            {
                { IsRecord: true, IsReferenceType: true } => "record class",
                { IsRecord: true, IsValueType: true } => "record struct",
                { IsReferenceType: true } => "class",
                { IsValueType: true } => "struct",
                _ => String.Empty
            };

            var source = _sourceTemplate.Replace(_declarationKeywordPlaceholder, declarationKeyword).Replace(_sourceTextMacro, quotedSourceText);

#pragma warning disable IDE0045 // Convert to conditional expression
            if(context.Attributes.Length > 0 &&
               context.Attributes[0].NamedArguments.Length > 0 &&
               context.Attributes[0].NamedArguments[0].Key == "OmitTypeCheck" &&
               context.Attributes[0].NamedArguments[0].Value.Value is Boolean omitTypeCheck &&
               omitTypeCheck)
            {
                source = source.Replace(_typeCheckPlaceholder, "");
            } else
            {
                source = source.Replace(_typeCheckPlaceholder,
                    """
                                if(data.AttributeClass != null &&
                                   data.AttributeClass.MetadataName == "{METADATANAME}" &&
                                   data.AttributeClass.ContainingNamespace.Name == "{NAMESPACE}")
                                {
                                    return false;
                                }
                    """);
            }
#pragma warning restore IDE0045 // Convert to conditional expression

            var result = new AttributeSourceModel(source, declaration, semanticModel, symbol);

            return result;
        }

        private static readonly Regex _rawStringValuePattern = new("\"*", RegexOptions.Compiled);

        private static String GetRawSourceTextValue(String sourceText)
        {
            var longestMatch = _rawStringValuePattern.Matches(sourceText)
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
        public static Boolean operator !=(AttributeSourceModel left, AttributeSourceModel right) => !( left == right );
    }
}
