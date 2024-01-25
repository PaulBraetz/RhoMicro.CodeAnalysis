namespace RhoMicro.CodeAnalysis.DocReflect;

using RhoMicro.CodeAnalysis.DocReflect.Comments;
using RhoMicro.CodeAnalysis.Library;

using System.Collections.Generic;

/// <summary>
/// Represents a types documentation.
/// </summary>
public partial class TypeDocumentation : Documentation
{
    /// <summary>
    /// Initializes a new instance.
    /// </summary>
    /// <param name="topLevelComments">
    /// <inheritdoc cref="Documentation(IEnumerable{DocumentationComment})"/>
    /// </param>
    /// <param name="typeParameters">
    /// The types type parameters.
    /// </param>
    public TypeDocumentation(
        IEnumerable<DocumentationComment> topLevelComments,
        IEnumerable<Typeparam> typeParameters)
        : base(topLevelComments)
        => TypeParameters = typeParameters.ToEquatableNameMap(p => p.Name, nameof(typeParameters));

    /// <summary>
    /// Gets an empty type documentation comment.
    /// </summary>
    public static TypeDocumentation Empty { get; } =
        new(Array.Empty<DocumentationComment>(), Array.Empty<Typeparam>());

    /// <summary>
    /// Gets the type parameter comments; in order of declaration.
    /// </summary>
    public IReadOnlyDictionary<String, Typeparam> TypeParameters { get; }
}
