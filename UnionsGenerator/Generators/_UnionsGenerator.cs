namespace RhoMicro.CodeAnalysis.UnionsGenerator.Generators;

using Microsoft.CodeAnalysis;

#pragma warning disable IDE1006 // Naming Styles
sealed class _UnionsGenerator : IIncrementalGenerator
#pragma warning restore IDE1006 // Naming Styles
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(static c => c.AddSource($"{nameof(UnionFactoryAttribute)}.g.cs", UnionFactoryAttribute.SourceText));
        context.RegisterPostInitializationOutput(static c => c.AddSource($"{nameof(UnionTypeAttribute)}.g.cs", UnionTypeAttribute.SourceText));
        context.RegisterPostInitializationOutput(static c => c.AddSource($"{nameof(RelationAttribute)}.g.cs", RelationAttribute.SourceText));
        context.RegisterPostInitializationOutput(static c => c.AddSource($"{nameof(UnionTypeSettingsAttribute)}.g.cs", UnionTypeSettingsAttribute.SourceText));
    }
}
