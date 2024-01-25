namespace RhoMicro.CodeAnalysis.DocReflect.Generators;

using System.Collections.Generic;
using System.Xml;

readonly struct ExtractCommentStepResult(String typeMetadataName, String rawComment, XmlDocument parsedComment)
    : IEquatable<ExtractCommentStepResult>
{
    public readonly String TypeMetadataName { get; } = typeMetadataName;
    private readonly String _rawComment = rawComment;
    public readonly XmlDocument ParsedComment { get; } = parsedComment;

    public override Boolean Equals(Object? obj) => obj is ExtractCommentStepResult result && Equals(result);
    public Boolean Equals(ExtractCommentStepResult other) => TypeMetadataName == other.TypeMetadataName && _rawComment == other._rawComment;

    public override Int32 GetHashCode()
    {
        var hashCode = -15450755;
        hashCode = hashCode * -1521134295 + EqualityComparer<String>.Default.GetHashCode(TypeMetadataName);
        hashCode = hashCode * -1521134295 + EqualityComparer<String>.Default.GetHashCode(_rawComment);
        return hashCode;
    }

    public static Boolean operator ==(ExtractCommentStepResult left, ExtractCommentStepResult right) => left.Equals(right);
    public static Boolean operator !=(ExtractCommentStepResult left, ExtractCommentStepResult right) => !(left == right);
}
