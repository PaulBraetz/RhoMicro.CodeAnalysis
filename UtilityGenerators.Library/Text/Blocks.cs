namespace RhoMicro.CodeAnalysis.Library.Text;

static partial class Blocks
{
    public static Block Indent { get; } = new();
    public static Block Braces { get; } = new("{\n", "}\n", PlaceDelimitersOnNewLine: true);
    public static Block Parens { get; } = new('(', ')', Indentation: StringOrChar.Empty);
    public static Block Brackets { get; } = new('[', ']', Indentation: StringOrChar.Empty);
    public static Block Angled { get; } = new('<', '>', Indentation: StringOrChar.Empty);
    public static Block Region(String name) => new($"#region {name}\n", "#endregion\n", PlaceDelimitersOnNewLine: true, Indentation: StringOrChar.Empty);
}
