namespace RhoMicro.CodeAnalysis.Library.Text;
readonly partial record struct Block
{
    public static class Comments
    {
        public static Block Multiline { get; } = new("/*\n", "*/", PlaceDelimitersOnNewLine: true);
        public static Block SingleLine { get; } = new(
            $"// ",
            StringOrChar.Empty,
            PlaceDelimitersOnNewLine: true,
            Indentation: StringOrChar.CommentSlashes);

        public static Block Summary { get; } = TopLevelDoc("summary");
        public static Block Returns { get; } = TopLevelDoc("returns");
        public static Block Remarks { get; } = TopLevelDoc("remarks");
        public static Block Param(String name) => TopLevelDoc("param", "name", name);
        public static Block TypeParam(String name) => TopLevelDoc("typeparam", "name", name);
        public static Block TopLevelDoc(String name) => new(
            $"/// <{name}>\n",
            $"/// </{name}>\n",
            PlaceDelimitersOnNewLine: true,
            Indentation: StringOrChar.DocCommentSlashes);
        public static Block TopLevelDoc(String name, String attributeName, String attributeValue)
        {
            var block = TopLevelDoc(name);
            var resut = block with
            {
                OpeningDelimiter = $"/// <{name} {attributeName}=\"{attributeValue}\">\n"
            };

            return resut;
        }

        public static Block Paragraph { get; } = Doc("para");
        public static Block Code { get; } = Doc("c");
        public static Block Doc(String name) => new(
            $"<{name}>\n",
            $"</{name}>\n",
            PlaceDelimitersOnNewLine: true,
            Indentation: StringOrChar.Empty);
        public static Block Doc(String name, String attributeName, String attributeValue) =>
            Doc(name) with
            {
                OpeningDelimiter = $"<{name} {attributeName}=\"{attributeValue}\">\n"
            };
    }
}
