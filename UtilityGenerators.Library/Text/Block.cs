namespace RhoMicro.CodeAnalysis.Library.Text;

using Microsoft.CodeAnalysis.VisualBasic.Syntax;

readonly partial record struct Block(
    StringOrChar OpeningDelimiter = default,
    StringOrChar ClosingDelimiter = default,
    Boolean PlaceDelimitersOnNewLine = false,
    StringOrChar? Indentation = null)
{
    public static Block Indent { get; } = new();
    public static Block Braces { get; } = new("{\n", "}\n", PlaceDelimitersOnNewLine: true);
    public static Block Parens { get; } = new('(', ')');
    public static Block Brackets { get; } = new('[', ']');
    public static Block Angled { get; } = new('<', '>');
    public static Block Region(String name) => new($"#region {name}\n", "#endregion\n", PlaceDelimitersOnNewLine: true);
}
