namespace RhoMicro.CodeAnalysis.UnionsGenerator.Generators.Expansions;

using RhoMicro.CodeAnalysis.UnionsGenerator.Models;

internal sealed class ConversionFunctionsCache(TargetDataModel model) : ExpansionBase(model, Macro.ConversionFunctionsCache)
{
    protected override void Expand(ExpandingMacroBuilder builder)
    {
        _ = builder *
            "file static class " * Model.ConversionFunctionsTypeName /
            """
            {
                public static readonly System.Collections.Concurrent.ConcurrentDictionary<Type, Object> Cache = new();
            }

            """;
    }
}
