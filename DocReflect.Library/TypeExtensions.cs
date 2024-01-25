namespace RhoMicro.CodeAnalysis.DocReflect;

using DocReflect.Infrastructure;

using System.Collections.Concurrent;
using System.ComponentModel;
using System.Reflection;

/// <summary>
/// Contains reflection extensions required to access type, method and property documentation.
/// </summary>
public static partial class Extensions
{
    private static readonly Lazy<List<IDocumentationProvider>> _providers =
        new(() => AppDomain.CurrentDomain
            .GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t =>
                t.CustomAttributes.Any(a => a.AttributeType == typeof(DocumentationProviderAttribute)) &&
                typeof(IDocumentationProvider).IsAssignableFrom(t) &&
                t.GetConstructors().Any(c => c.IsPublic && c.GetParameters().Length == 0))
            .Distinct()
            .Select(Activator.CreateInstance)
            .OfType<IDocumentationProvider>()
            .ToList());
    private static readonly Lazy<Dictionary<Type, TypeDocumentation>> _typeDocs =
        new(() => _providers.Value
            .SelectMany(p =>
            {
                try
                {
                    return p.GetTypeDocumentations();
                } catch
                {
                    return Array.Empty<KeyValuePair<Type, TypeDocumentation>>();
                }
            })
            .Distinct(new KeyValuePairComparer<Type, TypeDocumentation>())
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value));
    private static readonly Lazy<Dictionary<MethodInfo, MethodDocumentation>> _methodDocs =
        new(() => _providers.Value
            .SelectMany(p =>
            {
                try
                {
                    return p.GetMethodDocumentations();
                } catch
                {
                    return Array.Empty<KeyValuePair<MethodInfo, MethodDocumentation>>();
                }
            })
            .Distinct(new KeyValuePairComparer<MethodInfo, MethodDocumentation>())
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value));
    private static readonly Lazy<Dictionary<PropertyInfo, PropertyDocumentation>> _propertyDocs =
        new(() => _providers.Value
            .SelectMany(p =>
            {
                try
                {
                    return p.GetPropertyDocumentations();
                } catch
                {
                    return Array.Empty<KeyValuePair<PropertyInfo, PropertyDocumentation>>();
                }
            })
            .Distinct(new KeyValuePairComparer<PropertyInfo, PropertyDocumentation>())
            .ToDictionary(kvp => kvp.Key, kvp => kvp.Value));

    /// <summary>
    /// Gets the documentation available for this type.
    /// </summary>
    /// <param name="type">The type whose documentation to locate.</param>
    /// <returns>
    /// The documentation located for <paramref name="type"/>,
    /// if one could be located; otherwise, <see cref="TypeDocumentation.Empty"/>.</returns>
    public static TypeDocumentation GetDocumentation(this Type type)
    {
        var result = _typeDocs.Value.TryGetValue(type, out var r) ?
            r :
            TypeDocumentation.Empty;

        return result;
    }
    /// <summary>
    /// Gets the documentation available for this method.
    /// </summary>
    /// <param name="method">The method whose documentation to locate.</param>
    /// <returns>
    /// The documentation located for <paramref name="method"/>,
    /// if one could be located; otherwise, <see cref="MethodDocumentation.Empty"/>.</returns>
    public static MethodDocumentation GetDocumentation(this MethodInfo method)
    {
        var result = _methodDocs.Value.TryGetValue(method, out var r) ?
            r :
            MethodDocumentation.Empty;

        return result;
    }
    /// <summary>
    /// Gets the documentation available for this property.
    /// </summary>
    /// <param name="property">The property whose documentation to locate.</param>
    /// <returns>
    /// The documentation located for <paramref name="property"/>,
    /// if one could be located; otherwise, <see cref="PropertyDocumentation.Empty"/>.</returns>
    public static PropertyDocumentation GetDocumentation(this PropertyInfo property)
    {
        var result = _propertyDocs.Value.TryGetValue(property, out var r) ?
            r :
            PropertyDocumentation.Empty;

        return result;
    }
}
