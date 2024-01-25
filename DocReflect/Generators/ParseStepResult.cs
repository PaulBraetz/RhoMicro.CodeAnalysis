namespace RhoMicro.CodeAnalysis.DocReflect.Generators;

using System.Collections.Generic;

readonly struct ParseStepResult(String typeMetadataName, String rawComment, TypeDocumentation documentation) : IEquatable<ParseStepResult>
{
    public readonly String TypeMetadataName { get; } = typeMetadataName;
    private readonly String _rawComment = rawComment;

    public readonly TypeDocumentation Documentation { get; } = documentation;

    public override Boolean Equals(Object? obj) => obj is ParseStepResult result && Equals(result);
    public Boolean Equals(ParseStepResult other) => TypeMetadataName == other.TypeMetadataName && _rawComment == other._rawComment;

    public override Int32 GetHashCode()
    {
        var hashCode = 462352912;
        hashCode = hashCode * -1521134295 + EqualityComparer<String>.Default.GetHashCode(TypeMetadataName);
        hashCode = hashCode * -1521134295 + EqualityComparer<String>.Default.GetHashCode(_rawComment);
        return hashCode;
    }

    public static Boolean operator ==(ParseStepResult left, ParseStepResult right) => left.Equals(right);
    public static Boolean operator !=(ParseStepResult left, ParseStepResult right) => !(left == right);
}
