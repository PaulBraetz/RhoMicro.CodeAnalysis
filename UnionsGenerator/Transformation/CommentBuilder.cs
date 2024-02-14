namespace RhoMicro.CodeAnalysis.Library.Text;
using System;

using RhoMicro.CodeAnalysis.UnionsGenerator.Models;

partial record CommentBuilder
{
    public IndentedStringBuilder SeeRef(UnionTypeModel model) =>
        SeeRef(model.Signature);
    public IndentedStringBuilder SeeRef(TypeSignatureModel signature) =>
        SeeRef(signature.Names.CommentRefString);
    public IndentedStringBuilder Ref(TypeSignatureModel typeSignature) =>
        typeSignature.IsTypeParameter ?
        Builder.Comment.TypeParamRef(typeSignature.Names.CommentRefString) :
        Builder.Comment.SeeRef(typeSignature.Names.CommentRefString);
    public IndentedStringBuilder InternalUse(TypeSignatureModel signature) =>
        Builder.Comment.OpenRemarks()
        .Append("This member is not intended for use by user code inside of or any code outside of ").Comment.Ref(signature).Append('.')
        .CloseBlock();
}
