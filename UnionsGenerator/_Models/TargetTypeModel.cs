namespace RhoMicro.CodeAnalysis.UnionsGenerator._Models;

using Microsoft.CodeAnalysis;

using System.Threading;

record TargetTypeModel(String Namespace, EquatableList<String> ContainingTypeNames, String DeclarationKeyword, String DeclarationName)
{
    private static readonly SymbolDisplayFormat _nameFormat =
        new(typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameOnly,
            genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters | SymbolDisplayGenericsOptions.IncludeVariance,
            miscellaneousOptions: SymbolDisplayMiscellaneousOptions.IncludeNullableReferenceTypeModifier);

    public static TargetTypeModel Create(INamedTypeSymbol target, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var @namespace = target.ContainingNamespace.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        var containingTypeNames = GetContainingTypeNames(target, cancellationToken);
        var declarationKeyword = target.TypeKind == TypeKind.Class ?
            "class" :
            target.TypeKind == TypeKind.Struct ?
            "struct" :
            throw new InvalidOperationException("Unable to construct target type model for interfaces.");
        var declarationName = target.ToDisplayString(_nameFormat);

        TargetTypeModel result;
        if(target.TypeParameters.Length > 0)
        {
            var typeParameterNames = GetTypeParameterNames(target, cancellationToken);
            result = new GenericTargetTypeModel(
                @namespace,
                containingTypeNames,
                declarationKeyword,
                declarationName,
                typeParameterNames);
        } else
        {
            result = new TargetTypeModel(@namespace, containingTypeNames, declarationKeyword, declarationName);
        }

        return result;
    }
    private static EquatableList<String> GetTypeParameterNames(INamedTypeSymbol target, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var names = new List<String>();
        for(var i = 0; i < target.TypeParameters.Length; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            names.Add(target.TypeParameters[i].Name);
        }

        return new(names);
    }
    private static EquatableList<String> GetContainingTypeNames(INamedTypeSymbol target, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var names = new List<String>();
        appendSignature(target.ContainingType);
        return new(names);

        void appendSignature(ITypeSymbol symbol)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if(symbol.ContainingType != null)
                appendSignature(symbol.ContainingType);

            var kind = symbol switch
            {
                { IsRecord: true, TypeKind: TypeKind.Class } => "record class",
                { IsRecord: true, TypeKind: TypeKind.Struct } => "record struct",
                { TypeKind: TypeKind.Class } => "class ",
                { TypeKind: TypeKind.Struct } => "struct ",
                { TypeKind: TypeKind.Interface } => "interface ",
                _ => null
            };
            if(kind == null)
                return;

            var signature = $"partial {kind} {symbol.ToDisplayString(_nameFormat)}";
            names!.Add(signature);
        }
    }
}
