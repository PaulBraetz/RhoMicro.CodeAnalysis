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
        INamedTypeSymbol symbol)
        : base(annotations, operatorOmissions, symbol)
    {
        TargetDeclaration = targetDeclaration;
        SemanticModel = semanticModel;

        ValueTypeContainerName = $"__{Symbol.ToIdentifierCompatString()}_ValueTypeContainer";
    }

    public readonly TypeDeclarationSyntax TargetDeclaration;
    public readonly SemanticModel SemanticModel;
    public readonly String ValueTypeContainerName;
    public readonly String TagTypeName = "__Tag";

    public static TargetDataModel Create(TypeDeclarationSyntax targetDeclaration, SemanticModel semanticModel)
    {
        var targetSymbol = semanticModel.GetDeclaredSymbol(targetDeclaration) as INamedTypeSymbol ??
            throw new ArgumentException(
                $"targetDeclaration {targetDeclaration.Identifier.Text} could not be retrieved as an instance of ITypeSymbol from the semantic model provided.",
                nameof(targetDeclaration));

        var (annotations, omissions) = CreateModels(targetSymbol);

        var result = new TargetDataModel(
            targetDeclaration,
            semanticModel,
            annotations,
            omissions,
            targetSymbol);

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
