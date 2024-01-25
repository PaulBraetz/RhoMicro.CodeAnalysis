namespace RhoMicro.CodeAnalysis.DslGenerator.Lexing;

using System.Text.RegularExpressions;

#if DSL_GENERATOR
[IncludeFile]
#endif
[UnionType(typeof(String))]
[UnionType(typeof(Char))]
[UnionType(typeof(StringSlice))]
[UnionTypeSettings(ToStringSetting = ToStringSetting.Simple, EmitGeneratedSourceCode = true)]
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
            s => s.Equals(c),
            thisChar => thisChar == c,
            s => s.Length == 1 && s[0] == c);
    public Boolean Equals(String s) =>
        Match(
            slice => slice.Equals(s),
            c => s.Length == 1 && s[0] == c,
            thisString => thisString == s);
    public Boolean Equals(StringSlice s) =>
        Match(s.Equals, s.Equals, s.Equals);
    public String ToEscapedString() =>
        ToString()?
        .Replace("\n", "\\n")
        .Replace("\r", "\\r")
        .Replace("\t", "\\t")
        ?? String.Empty;
}
