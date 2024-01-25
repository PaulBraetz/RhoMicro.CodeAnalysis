namespace RhoMicro.CodeAnalysis.DocReflect.Comments;

/// <summary>
/// Represents a <c>summary</c> comment.
/// </summary>
public sealed partial record Summary : DocumentationComment
{
    /// <summary>
    /// Initializes a new instance.
    /// </summary>
    /// <param name="content"><inheritdoc cref="DocumentationComment.DocumentationComment(CommentContents)"/></param>
    public Summary(CommentContents content) : base(content)
    {
    }
}
