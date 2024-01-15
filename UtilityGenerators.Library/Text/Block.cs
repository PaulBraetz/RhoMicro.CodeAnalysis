namespace RhoMicro.CodeAnalysis.Library.Text;
readonly record struct Block(
    StringOrChar OpeningDelimiter,
    StringOrChar ClosingDelimiter,
    Boolean AppendNewLine = false,
    Boolean IncreaseIndentation = true)
{
    public static Block Braces { get; } = new('{', '}');
    public static Block BracesBreak { get; } = new('{', '}', AppendNewLine: true);
    public static Block Parens { get; } = new('(', ')');
    public static Block ParensBreak { get; } = new('(', ')', AppendNewLine: true);
    public static Block Brackets { get; } = new('[', ']');
    public static Block BracketsBreak { get; } = new('[', ']', AppendNewLine: true);
    public static Block Angled { get; } = new('<', '>');
    public static Block AngledBreak { get; } = new('<', '>', AppendNewLine: true);
    public static Block MultilineComment { get; } = new("/*", "*/", AppendNewLine: true);
    public static Block Region(String name) => new($"#region {name}", "#endregion", AppendNewLine: true, IncreaseIndentation: false);
    public static Block DocComment(String name) => new($"/// <{name}>", $"/// </{name}>", AppendNewLine: true, IncreaseIndentation: false);
    public static Block DocComment(String name, String attributeName, String attributeValue) => new($"/// <{name} {attributeName}=\"{attributeValue}\">", $"/// </{name}>", AppendNewLine: true, IncreaseIndentation: false);
}
