namespace RhoMicro.CodeAnalysis.UnionsGenerator;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using System;

static class Util
{
    public static Boolean IsGeneratorTarget(SyntaxNode node) =>
        node is TypeDeclarationSyntax tds and not RecordDeclarationSyntax &&
        tds.Modifiers.Any(SyntaxKind.PartialKeyword);

    public static Boolean IsAnalysisCandidate(SyntaxNode node, SemanticModel semanticModel)
    {
        if(semanticModel.GetDeclaredSymbol(node) is not INamedTypeSymbol named)
            return false;

        var attributes = named.GetAttributes();
        foreach(var attribute in attributes)
        {
            if(UnionTypeAttribute.TryCreate(attribute, out _))
                return true;
            if(RelationAttribute.TryCreate(attribute, out _))
                return true;
            if(UnionTypeSettingsAttribute.TryCreate(attribute, out _))
                return true;
        }

        var result = named.GetMembers().SelectMany(m => m.GetAttributes()).OfUnionFactoryAttribute().Any();

        return result;
    }
}
