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
}
