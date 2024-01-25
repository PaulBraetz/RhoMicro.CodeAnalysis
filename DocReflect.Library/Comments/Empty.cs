namespace RhoMicro.CodeAnalysis.DocReflect.Comments;
using System;

/// <summary>
/// Represents an empty or non-existent top-level documentation comment.
/// </summary>
public sealed partial record Empty : DocumentationComment
{
    /// <summary>
    /// Initializes a new instance.
    /// </summary>
    public Empty() : base(CommentContents.Empty)
    { }
}
