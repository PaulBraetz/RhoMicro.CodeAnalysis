namespace RhoMicro.CodeAnalysis.DocReflect;

using RhoMicro.CodeAnalysis.DocReflect.Comments;
using RhoMicro.CodeAnalysis.Library;

using System;

/// <summary>
/// Represents a members documentation.
/// </summary>
public partial class Documentation : IEquatable<Documentation?>
{
    /// <summary>
    /// Initializes a new instance.
    /// </summary>
    /// <param name="topLevelComments">All top level documentation comments; in order of declaration.</param>
    public Documentation(IEnumerable<DocumentationComment> topLevelComments)
    {
        _ = topLevelComments ?? throw new ArgumentNullException(nameof(topLevelComments));

        var tlcList = new List<DocumentationComment>();

        foreach(var tlc in topLevelComments)
        {
            if(tlc is Summary summary)
                Summary ??= summary;
            else if(tlc is Remarks remarks)
                Remarks ??= remarks;
            else if(tlc is Example example)
                Example ??= example;

            tlcList.Add(tlc);
        }

        TopLevelComments = tlcList.AsEquatable();
    }

    /// <summary>
    /// Gets all top level documentation comments; in order of declaration.
    /// </summary>
    public IReadOnlyList<DocumentationComment> TopLevelComments { get; }
    /// <summary>
    /// Gets the summary comment if one exists; otherwise, <see langword="null"/>.
    /// </summary>
    public Summary? Summary { get; }
    /// <summary>
    /// Gets the remarks comment if one exists; otherwise, <see langword="null"/>.
    /// </summary>
    public Remarks? Remarks { get; }
    /// <summary>
    /// Gets the example comment if one exists; otherwise, <see langword="null"/>.
    /// </summary>
    public Example? Example { get; }
    /// <inheritdoc/>
    public override Boolean Equals(Object? obj) => Equals(obj as Documentation);
    /// <inheritdoc/>
    public Boolean Equals(Documentation? other) => other is not null &&
        TopLevelComments.Count == other.TopLevelComments.Count &&
        TopLevelComments.Equals(other.TopLevelComments);
    /// <inheritdoc/>
    public override Int32 GetHashCode() => TopLevelComments.GetHashCode();
}
