namespace RhoMicro.CodeAnalysis.DocReflect.Comments;

/// <summary>
/// Represents a <c>typeparam</c> comment.
/// </summary>
public sealed partial record Typeparam : DocumentationComment
{
    /// <summary>
    /// Initializes a new instance.
    /// </summary>
    /// <param name="content"><inheritdoc cref="DocumentationComment.DocumentationComment(CommentContents)"/></param>
    /// <param name="name">The name of the type parameter referenced.</param>
    public Typeparam(CommentContents content, String name)
        : base(content)
        => Name = name;
    /// <summary>
    /// Gets the name of the type parameter referenced.
    /// </summary>
    public String Name { get; }
}
