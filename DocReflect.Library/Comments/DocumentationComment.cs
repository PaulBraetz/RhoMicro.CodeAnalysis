namespace RhoMicro.CodeAnalysis.DocReflect.Comments;

/// <summary>
/// Represents a top level documentation comment attached to some member.
/// </summary>
/// <param name="Contents">
/// The contents of this comment.
/// </param>
public abstract partial record DocumentationComment(CommentContents Contents);