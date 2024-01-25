namespace RhoMicro.CodeAnalysis.DocReflect.Comments;
using System;

/// <summary>
/// Represents a <c>param</c> comment.
/// </summary>
public sealed partial record Param : DocumentationComment
{
    /// <summary>
    /// Initializes a new instance.
    /// </summary>
    /// <param name="contents"><inheritdoc cref="DocumentationComment(CommentContents)"/></param>
    /// <param name="name">The name of the parameter referenced.</param>
    public Param(CommentContents contents, String name)
        : base(contents)
        => Name = name;
    /// <summary>
    /// Gets the name of the parameter referenced.
    /// </summary>
    public String Name { get; }
}
