namespace RhoMicro.CodeAnalysis.UnionsGenerator.Models;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

using RhoMicro.CodeAnalysis;
using RhoMicro.CodeAnalysis.UnionsGenerator.Generators.Expansions;

using System;

internal sealed class TargetDataModel : UnionDataModel
{
    private TargetDataModel(
        TypeDeclarationSyntax targetDeclaration,
        SemanticModel semanticModel,
        AnnotationDataModel annotations,
        OperatorOmissionModel operatorOmissions,
        INamedTypeSymbol symbol,
        Boolean implementsEquals)
        : base(annotations, operatorOmissions, symbol)
    {
        TargetDeclaration = targetDeclaration;
        SemanticModel = semanticModel;

        ValueTypeContainerName = $"__{Symbol.ToIdentifierCompatString()}_ValueTypeContainer";
        ConversionFunctionsTypeName = $"__{Symbol.ToIdentifierCompatString()}_ConversionFunctions";
        ImplementsEquals = implementsEquals;
    }

    public readonly TypeDeclarationSyntax TargetDeclaration;
    public readonly SemanticModel SemanticModel;
    public readonly String ValueTypeContainerName;
    public readonly String TagTypeName = "__Tag";
    public readonly String ConversionFunctionsTypeName;
    public readonly Boolean ImplementsEquals;

    public static TargetDataModel Create(TypeDeclarationSyntax targetDeclaration, SemanticModel semanticModel)
    {
        var targetSymbol = semanticModel.GetDeclaredSymbol(targetDeclaration) as INamedTypeSymbol ??
            throw new ArgumentException(
                $"targetDeclaration {targetDeclaration.Identifier.Text} could not be retrieved as an instance of ITypeSymbol from the semantic model provided.",
                nameof(targetDeclaration));

        var (annotations, omissions) = CreateModels(targetSymbol);

        var implementsEquals = targetSymbol.GetMembers()
            .OfType<IMethodSymbol>()
            .Any(s =>
                s.Name == nameof(Equals) &&
                s.Parameters.Length == 1 &&
                s.Parameters[0].Type.Equals(targetSymbol, SymbolEqualityComparer.Default) &&
                targetSymbol.Equals(s.ContainingSymbol, SymbolEqualityComparer.Default));

        var result = new TargetDataModel(
            targetDeclaration,
            semanticModel,
            annotations,
            omissions,
            targetSymbol,
            implementsEquals);

        return result;
    }
    public String GetSpecificAccessibility(RepresentableTypeModel representableType)
    {
        var accessibility = Annotations.Settings.ConstructorAccessibility;

        if(accessibility == ConstructorAccessibilitySetting.PublicIfInconvertible &&
           OperatorOmissions.AllOmissions.Contains(representableType))
        {
            accessibility = ConstructorAccessibilitySetting.Public;
        }

        var result = accessibility == ConstructorAccessibilitySetting.Public ?
            "public" :
            "private";

        return result;
    }
}
