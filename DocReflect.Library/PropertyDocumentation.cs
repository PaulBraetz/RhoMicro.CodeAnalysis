namespace RhoMicro.CodeAnalysis.DocReflect;

using RhoMicro.CodeAnalysis.DocReflect.Comments;

using System.Collections.Generic;

/// <summary>
/// Represents a properties documentation.
/// </summary>
public partial class PropertyDocumentation : Documentation
{
    /// <summary>
    /// Initializes a new instance.
    /// </summary>
    /// <param name="topLevelComments">
    /// <inheritdoc cref="Documentation(IEnumerable{DocumentationComment})"/>
    /// </param>
    public PropertyDocumentation(IEnumerable<DocumentationComment> topLevelComments)
        : base(topLevelComments)
    { }

    /// <summary>
    /// Gets an empty type documentation comment.
    /// </summary>
    public static PropertyDocumentation Empty { get; } =
        new(Array.Empty<DocumentationComment>());
}
