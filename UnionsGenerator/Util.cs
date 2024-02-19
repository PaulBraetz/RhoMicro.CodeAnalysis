namespace RhoMicro.CodeAnalysis.UnionsGenerator;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using RhoMicro.CodeAnalysis.UnionsGenerator.Utils;

using System;

internal static class Util
{
    public static Boolean IsGeneratorTarget(SyntaxNode node) =>
        node is TypeDeclarationSyntax tds and not RecordDeclarationSyntax &&
        tds.Modifiers.Any(SyntaxKind.PartialKeyword) ||
        node is TypeParameterSyntax;

    public static Boolean IsUnionTypeSymbol(INamedTypeSymbol symbol, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var attributes = symbol.GetAttributes();
        foreach(var attribute in attributes)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if(Qualifications.IsUnionTypeAttribute(attribute)
                || Qualifications.IsRelationAttribute(attribute)
                || Qualifications.IsUnionTypeSettingsAttribute(attribute))
            {
                return true;
            }
        }

        cancellationToken.ThrowIfCancellationRequested();
        var result = symbol is INamedTypeSymbol named &&
            named.GetMembers()
                .OfType<IMethodSymbol>()
                .Any(Qualifications.IsUnionTypeFactorySymbol);

        return result;
    }
}
