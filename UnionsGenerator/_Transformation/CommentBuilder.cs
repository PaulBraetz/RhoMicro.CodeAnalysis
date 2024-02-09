﻿namespace RhoMicro.CodeAnalysis.Library.Text;
using System;

using RhoMicro.CodeAnalysis.UnionsGenerator._Models;

partial record CommentBuilder
{
    public IndentedStringBuilder SeeRef(UnionTypeModel target) =>
        SeeRef(target.Signature.Names.CommentRefString);
    public IndentedStringBuilder Ref(TypeSignatureModel typeSignature) =>
        typeSignature.IsTypeParameter ?
        Builder.Comment.TypeParamRef(typeSignature.Names.CommentRefString) :
        Builder.Comment.SeeRef(typeSignature.Names.CommentRefString);
    public IndentedStringBuilder InternalUse(UnionTypeModel target) =>
        Builder.Comment.OpenRemarks()
        .Append("This member is not intended for use by user code inside of or any code outside of ").Comment.Ref(target.Signature).Append('.')
        .CloseBlock();
}
