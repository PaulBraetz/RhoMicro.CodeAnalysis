
namespace RhoMicro.CodeAnalysis.UnionsGenerator._Transformation.Visitors
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;

    using RhoMicro.CodeAnalysis.UnionsGenerator._Models;
    using RhoMicro.CodeAnalysis.UnionsGenerator.Generators;

    partial class AppendableSourceText
    {
        public static AppendableSourceText Create(String declaredSource, String unionTypeName)
        {
            var symbol = CSharpCompilation.Create(
                assemblyName: null,
                [
                    CSharpSyntaxTree.ParseText(declaredSource),
                    CSharpSyntaxTree.ParseText(UnionTypeBaseAttribute.SourceText),
                    CSharpSyntaxTree.ParseText(UnionTypeSettingsAttribute.SourceText),
                    CSharpSyntaxTree.ParseText(RelationAttribute<Object>.SourceText),
                    CSharpSyntaxTree.ParseText(UnionTypeFactoryAttribute.SourceText)
                ],
                options: new(OutputKind.DynamicallyLinkedLibrary))
                .GetSymbolsWithName(unionTypeName, SymbolFilter.Type)
                .OfType<INamedTypeSymbol>()
                .Single();

            var assemblySettings = symbol.ContainingModule.ContainingAssembly.GetAttributes()
                .Where(d => MatchesMetadataName(d, UnionTypeSettingsAttribute.MetadataName))
                .Select(SettingsAttributeData.CreateAssemblySettingsWithDefaults)
                .SingleOrDefault() ?? SettingsAttributeData.Default;

            var implementsToString = symbol.GetMembers("ToString")
                .OfType<IMethodSymbol>()
                .Any(m => m.Parameters.Length == 0 && m.ReturnType.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat) == "string");

#nullable disable
            var factories = symbol.GetMembers()
                .OfType<IMethodSymbol>()
                .Where(m => m.Parameters.Length == 1 &&
                    m.IsStatic &&
                    SymbolEqualityComparer.IncludeNullability.Equals(symbol, m.ReturnType))
                .ToDictionary(
                    m => m.Parameters[0].Type,
                    m => FactoryModel.CreateCustom(m.Name, m.Parameters[0].Type, CancellationToken.None),
                    SymbolEqualityComparer.IncludeNullability);
#nullable restore

            var signature = TypeSignatureModel.Create(symbol, default);
            var genericRepresentableTypes = symbol.GetAttributes()
                .Where(d => MatchesMetadataName(d, UnionTypeBaseAttribute.GenericMetadataName))
                .Select(d => (success: UnionTypeBaseAttribute.TryCreate(d, out var a), attribute: a, type: d.AttributeClass!.TypeArguments[0]))
                .Where(t => t.success)
                .Select(t => (model: t.attribute!.GetPartialModel(new(t.type), CancellationToken.None), t.type))
                .Select(t => (t.model, factory: factories.TryGetValue(t.type, out var f) ? f : FactoryModel.CreateGenerated(t.model)))
                .Select(t => RepresentableTypeModel.Create(t.model, t.factory, CancellationToken.None));
            var nonGenericRepresentableTypes = symbol.TypeParameters
                .SelectMany(p => p.GetAttributes().Select(a => (parameter: p, attribute: a)))
                .Where(t => MatchesMetadataName(t.attribute, UnionTypeBaseAttribute.NonGenericMetadataName))
                .Select(t => (success: UnionTypeBaseAttribute.TryCreate(t.attribute, out var a), attribute: a, type: t.parameter))
                .Where(t => t.success)
                .Select(t => (model: t.attribute!.GetPartialModel(new(t.type), CancellationToken.None), t.type))
                .Select(t => (t.model, factory: factories.TryGetValue(t.type, out var f) ? f : FactoryModel.CreateGenerated(t.model)))
                .Select(t => RepresentableTypeModel.Create(t.model, t.factory, CancellationToken.None));
            var representableTypes = genericRepresentableTypes.Concat(nonGenericRepresentableTypes)
                .ToEquatableList(CancellationToken.None);

            var relations = symbol.GetAttributes()
                .Where(d => MatchesMetadataName(d, RelationAttribute<Object>.MetadataName))
                .Select(d => d.AttributeClass?.TypeArguments[0] as INamedTypeSymbol)
                .Where(t => t != null)
                .Select(t => RelationModel.Create(symbol, t!, CancellationToken.None))
                .ToEquatableList(CancellationToken.None);

            var settingsData = symbol.GetAttributes()
                .Where(d => MatchesMetadataName(d, UnionTypeSettingsAttribute.MetadataName))
                .Select(d => SettingsAttributeData.CreateDeclaredSettingsWithDefaults(d, assemblySettings))
                .Select(d => (success: UnionTypeSettingsAttribute.TryCreate(d, out var a), attribute: a))
                .Where(t => t.success)
                .Select(t => t.attribute)
                .SingleOrDefault() ?? new();
            var settings = SettingsModel.Create(settingsData, implementsToString);

            var target = UnionTypeModel.Create(signature, representableTypes, relations, settings, CancellationToken.None);

            var result = Create(target);

            return result;
        }

        static Boolean MatchesMetadataName(AttributeData data, String name) =>
            $"{data.AttributeClass?.ContainingNamespace?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat.WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted))}.{data.AttributeClass?.MetadataName}"
            == name;
    }
}