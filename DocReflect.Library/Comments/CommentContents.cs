namespace RhoMicro.CodeAnalysis.DocReflect.Comments;
using System;

/// <summary>
/// Represents a comments contents.
/// </summary>
/// <param name="Text">The textual contents of the comment.</param>
public readonly partial record struct CommentContents(String Text)
{
    /// <summary>
    /// Gets empty comment contents.
    /// </summary>
    public static CommentContents Empty { get; } = new(String.Empty);
}
