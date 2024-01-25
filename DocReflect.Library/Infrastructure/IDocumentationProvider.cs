namespace RhoMicro.CodeAnalysis.DocReflect.Infrastructure;

using System.Reflection;

/// <summary>
/// Provides documentation for types, methods and properties. 
/// In order to be considered by the infrastructure, 
/// provider types must be marked using <see cref="DocumentationProviderAttribute"/>
/// and have a parameterless constructor.
/// </summary>
public interface IDocumentationProvider
{
    /// <summary>
    /// Provides a map of documentations onto their types.
    /// </summary>
    /// <returns>Documentations associated with their respective type.</returns>
    IEnumerable<KeyValuePair<Type, TypeDocumentation>> GetTypeDocumentations();
    /// <summary>
    /// Provides a map of documentations onto their methods.
    /// </summary>
    /// <returns>Documentations associated with their respective method.</returns>
    IEnumerable<KeyValuePair<MethodInfo, MethodDocumentation>> GetMethodDocumentations();
    /// <summary>
    /// Provides a map of documentations onto their properties.
    /// </summary>
    /// <returns>Documentations associated with their respective property.</returns>
    IEnumerable<KeyValuePair<PropertyInfo, PropertyDocumentation>> GetPropertyDocumentations();
}