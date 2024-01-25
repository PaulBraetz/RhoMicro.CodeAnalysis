namespace RhoMicro.CodeAnalysis.DocReflect.Comments;

/// <summary>
/// Represents a <c>example</c> comment.
/// </summary>
public sealed partial record Example : DocumentationComment
{
    /// <summary>
    /// Initializes a new instance.
    /// </summary>
    /// <param name="content"><inheritdoc cref="DocumentationComment.DocumentationComment(CommentContents)"/></param>
    public Example(CommentContents content) : base(content)
    {
    }
}