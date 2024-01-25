namespace RhoMicro.CodeAnalysis.DocReflect;

using RhoMicro.CodeAnalysis.DocReflect.Comments;
using RhoMicro.CodeAnalysis.Library;

using System.Collections.Generic;

/// <summary>
/// Represents a methods documentation.
/// </summary>
public partial class MethodDocumentation : Documentation
{
    /// <summary>
    /// Initializes a new instance.
    /// </summary>
    /// <param name="topLevelComments">
    /// <inheritdoc cref="Documentation(IEnumerable{DocumentationComment})"/>
    /// </param>
    /// <param name="typeParameters">
    /// The methods type parameters.
    /// </param>
    /// <param name="parameters">
    /// The methods parameters.
    /// </param>
    public MethodDocumentation(
        IEnumerable<DocumentationComment> topLevelComments,
        IEnumerable<Typeparam> typeParameters,
        IEnumerable<Param> parameters)
        : base(topLevelComments)
    {
        TypeParameters = typeParameters.ToEquatableNameMap(p => p.Name, nameof(typeParameters));
        Parameters = parameters.ToEquatableNameMap(p => p.Name, nameof(parameters));
    }

    /// <summary>
    /// Gets an empty type documentation comment.
    /// </summary>
    public static MethodDocumentation Empty { get; } =
        new(Array.Empty<DocumentationComment>(), Array.Empty<Typeparam>(), Array.Empty<Param>());

    /// <summary>
    /// Gets the type parameter comments; in order of declaration.
    /// </summary>
    public IReadOnlyDictionary<String, Typeparam> TypeParameters { get; }
    /// <summary>
    /// Gets the parameter comments; in order of declaration.
    /// </summary>
    public IReadOnlyDictionary<String, Param> Parameters { get; }
}
