namespace RhoMicro.CodeAnalysis.DocReflect.Comments;

/// <summary>
/// Represents a <c>remarks</c> comment.
/// </summary>
public sealed partial record Remarks : DocumentationComment
{
    /// <summary>
    /// Initializes a new instance.
    /// </summary>
    /// <param name="content"><inheritdoc cref="DocumentationComment.DocumentationComment(CommentContents)"/></param>
    public Remarks(CommentContents content) : base(content)
    {
    }
}
