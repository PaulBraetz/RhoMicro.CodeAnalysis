namespace RhoMicro.CodeAnalysis.Library.Text;

partial record CommentBuilder(IndentedStringBuilder Builder)
{
    #region Open Block
    public IndentedStringBuilder OpenSummary() =>
        Builder.OpenBlock(CommentBlocks.Summary);
    public IndentedStringBuilder OpenReturns() =>
        Builder.OpenBlock(CommentBlocks.Returns);
    public IndentedStringBuilder OpenRemarks() =>
        Builder.OpenBlock(CommentBlocks.Remarks);
    public IndentedStringBuilder OpenParam(String name) =>
        Builder.OpenBlock(CommentBlocks.Param(name));
    public IndentedStringBuilder OpenTypeParam(String name) =>
        Builder.OpenBlock(CommentBlocks.TypeParam(name));

    public IndentedStringBuilder OpenList(String type) =>
        Builder.OpenBlock(CommentBlocks.List(type));
    public IndentedStringBuilder OpenItem() =>
        Builder.OpenBlock(CommentBlocks.Item);
    public IndentedStringBuilder OpenTerm() =>
        Builder.OpenBlock(CommentBlocks.Term);
    public IndentedStringBuilder OpenDescription() =>
        Builder.OpenBlock(CommentBlocks.Description);

    public IndentedStringBuilder OpenParagraph() =>
        Builder.OpenBlock(CommentBlocks.Paragraph);
    public IndentedStringBuilder OpenCode() =>
        Builder.OpenBlock(CommentBlocks.Code);

    public IndentedStringBuilder OpenDocBlock(String name) =>
        Builder.OpenBlock(CommentBlocks.Doc(name));
    public IndentedStringBuilder OpenDocBlock(String name, String attributeName, String attributeValue) =>
        Builder.OpenBlock(CommentBlocks.Doc(name, attributeName, attributeValue));
    public IndentedStringBuilder OpenSingleLineBlock() =>
        Builder.OpenBlock(CommentBlocks.SingleLine);
    public IndentedStringBuilder OpenMultilineBlock() =>
        Builder.OpenBlock(CommentBlocks.Multiline);
    #endregion
    #region Open Block Scope
    public BlockScope OpenSingleLineBlockScope() => new(OpenSingleLineBlock());
    public BlockScope OpenMultilineBlockScope() => new(OpenMultilineBlock());
    public BlockScope OpenDocBlockScope(String name) => new(OpenDocBlock(name));
    public BlockScope OpenDocBlockScope(String name, String attributeName, String attributeValue) =>
        new(OpenDocBlock(name, attributeName, attributeValue));
    #endregion
    #region Self Closing
    public IndentedStringBuilder SeeRef(String name) =>
        Builder.Append("<see cref=\"").Append(name).Append("\"/>");
    public IndentedStringBuilder Langword(String name) =>
        Builder.Append("<see langword=\"").Append(name).Append("\"/>");
    public IndentedStringBuilder InheritDoc(String name, Boolean topLevel = true)
    {
        if(topLevel)
            _ = Builder.Append("/// ");

        _ = Builder.Append("<inheritdoc cref=\"").Append(name).Append("\"/>");

        if(topLevel)
            _ = Builder.AppendLine();

        return Builder;
    }
    public IndentedStringBuilder InheritDoc(Boolean topLevel = true)
    {
        if(topLevel)
            _ = Builder.Append("/// ");

        _ = Builder.Append("<inheritdoc/>");

        if(topLevel)
            _ = Builder.AppendLine();

        return Builder;
    }
    public IndentedStringBuilder TypeParamRef(String name) =>
        Builder.Append("<typeparamref name=\"").Append(name).Append("\"/>");
    public IndentedStringBuilder ParamRef(String name) =>
        Builder.Append("<paramref name=\"").Append(name).Append("\"/>");
    public IndentedStringBuilder InternalUse(String name) =>
        Builder.Comment.OpenRemarks()
        .Append("This member is not intended for use by user code inside of or any code outside of ").Comment.SeeRef(name).Append('.')
        .CloseBlock();
    #endregion
}
