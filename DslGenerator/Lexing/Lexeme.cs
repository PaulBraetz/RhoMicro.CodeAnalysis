namespace RhoMicro.CodeAnalysis.DslGenerator.Lexing;

using System.Text.RegularExpressions;

#if DSL_GENERATOR
[IncludeFile]
#endif
[UnionType<String, Char, StringSlice>]
[UnionTypeSettings(
    ToStringSetting = ToStringSetting.Simple,
    Miscellaneous = MiscellaneousSettings.Default | MiscellaneousSettings.EmitGeneratedSourceCode)]
readonly partial struct Lexeme : IEquatable<String>, IEquatable<Char>, IEquatable<StringSlice>
{
    public Int32 Length => Match(
        s => s.Length,
        s => 1,
        s => s.Length);
    public static Lexeme Empty { get; } = String.Empty;
    public Boolean Equals(Lexeme other) =>
        Match(other.Equals, other.Equals, other.Equals);
    public override Int32 GetHashCode() =>
        Match(v => v.GetHashCode(), v => v.GetHashCode(), v => v.GetHashCode());
    public Boolean Equals(Char c) =>
        Match(
            s => s.Length == 1 && s[0] == c,
            thisChar => thisChar == c,
            s => s.Equals(c));
    public Boolean Equals(String s) =>
        Match(
            thisString => thisString == s,
            c => s.Length == 1 && s[0] == c,
            slice => slice.Equals(s));
    public Boolean Equals(StringSlice s) =>
        Match(s.Equals, s.Equals, s.Equals);
    public String ToEscapedString() =>
        ToString()?
        .Replace("\n", "\\n")
        .Replace("\r", "\\r")
        .Replace("\t", "\\t")
        ?? String.Empty;
}
