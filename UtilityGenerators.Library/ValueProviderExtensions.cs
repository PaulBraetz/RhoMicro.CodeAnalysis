namespace RhoMicro.CodeAnalysis.Library;

using Microsoft.CodeAnalysis;

using System.Collections.Immutable;

static class ValueProviderExtensions
{
    public static IncrementalValueProvider<ImmutableArray<T>> WithCollectionComparer<T>(this IncrementalValueProvider<ImmutableArray<T>> provider) =>
        provider.WithComparer(ImmutableArrayCollectionEqualityComparer<T>.Default);
    public static IncrementalValuesProvider<ImmutableArray<T>> WithCollectionComparer<T>(this IncrementalValuesProvider<ImmutableArray<T>> provider) =>
        provider.WithComparer(ImmutableArrayCollectionEqualityComparer<T>.Default);
}
