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
        Boolean implementsEquals,
        Boolean implementsGetHashCode)
        : base(annotations, operatorOmissions, symbol)
    {
        TargetDeclaration = targetDeclaration;
        SemanticModel = semanticModel;

        ValueTypeContainerName = $"__{Symbol.ToIdentifierCompatString()}_ValueTypeContainer";
        ConversionFunctionsTypeName = $"__{Symbol.ToIdentifierCompatString()}_ConversionFunctions";
        ImplementsEquals = implementsEquals;
        ImplementsGetHashCode = implementsGetHashCode;
    }

    public readonly TypeDeclarationSyntax TargetDeclaration;
    public readonly SemanticModel SemanticModel;
    public readonly String ValueTypeContainerName;
    public readonly String TagTypeName = "__Tag";
    public readonly String ConversionFunctionsTypeName;
    public readonly Boolean ImplementsEquals;
    public readonly Boolean ImplementsGetHashCode;

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

        var implementsGetHashCode = targetSymbol.GetMembers()
            .OfType<IMethodSymbol>()
            .Any(s =>
                s.Name == nameof(GetHashCode) &&
                s.Parameters.Length == 0 &&
                targetSymbol.Equals(s.ContainingSymbol, SymbolEqualityComparer.Default));

        var result = new TargetDataModel(
            targetDeclaration,
            semanticModel,
            annotations,
            omissions,
            targetSymbol,
            implementsEquals,
            implementsGetHashCode);

        return result;
    }
    public String GetSpecificAccessibility(RepresentableTypeModel _)
    {
        //commented out because of breaking rewrite changes
        return String.Empty;
        //var accessibility = Annotations.Settings.ConstructorAccessibility;

        //if(accessibility == ConstructorAccessibilitySetting.PublicIfInconvertible &&
        //   OperatorOmissions.AllOmissions.Contains(representableType))
        //{
        //    accessibility = ConstructorAccessibilitySetting.Public;
        //}

        //var result = accessibility == ConstructorAccessibilitySetting.Public ?
        //    "public" :
        //    "private";

        //return result;
    }
}
