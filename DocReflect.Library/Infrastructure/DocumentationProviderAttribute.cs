namespace RhoMicro.CodeAnalysis.DocReflect.Infrastructure;

/// <summary>
/// Marks a class as a provider of documentation.
/// In order to be used by the infrastructure, classes 
/// marked by this attribute must implement <see cref="IDocumentationProvider"/>
/// and have a parameterless constructor.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class DocumentationProviderAttribute : Attribute;
