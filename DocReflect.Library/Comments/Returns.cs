namespace RhoMicro.CodeAnalysis.DocReflect.Comments;

/// <summary>
/// Represents a <c>returns</c> comment.
/// </summary>
public sealed partial record Returns : DocumentationComment
{
    /// <summary>
    /// Initializes a new instance.
    /// </summary>
    /// <param name="content"><inheritdoc cref="DocumentationComment.DocumentationComment(CommentContents)"/></param>
    public Returns(CommentContents content) : base(content)
    {
    }
}
