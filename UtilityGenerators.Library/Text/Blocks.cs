namespace RhoMicro.CodeAnalysis.Library.Text;

static partial class Blocks
{
    public static Block Indent { get; } = new();
    public static Block Braces(StringOrChar newLine) => new($"{{{newLine}", $"}}{newLine}", PlaceDelimitersOnNewLine: true);
    public static Block Parens { get; } = new('(', ')', Indentation: StringOrChar.Empty);
    public static Block Brackets { get; } = new('[', ']', Indentation: StringOrChar.Empty);
    public static Block Angled { get; } = new('<', '>', Indentation: StringOrChar.Empty);
    public static Block Region(String name, StringOrChar newLine) => new($"#region {name}{newLine}", $"#endregion{newLine}", PlaceDelimitersOnNewLine: true, Indentation: StringOrChar.Empty);
}
