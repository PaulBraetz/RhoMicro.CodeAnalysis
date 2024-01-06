namespace RhoMicro.CodeAnalysis.DslGenerator.Lexing;

readonly record struct Token(TokenType Type, Lexeme Lexeme)
{
    public override String ToString() => $"[{Type}:{Lexeme.ToEscapedString()}]";
    public static Token CreateUnknown(Lexeme lexeme) => new(TokenType.Unknown, lexeme);
    public static Token CreateName(Lexeme lexeme) => new(TokenType.Name, lexeme);
    public static Token CreateWhitespace(Lexeme lexeme) => new(TokenType.Whitespace, lexeme);
    public static Token CreateSpecificRepetition(Lexeme lexeme) => new(TokenType.SpecificRepetition, lexeme);
    public static Token CreateComment(Lexeme lexeme) => new(TokenType.Comment, lexeme);
    public static Token CreateTerminal(Lexeme lexeme) => new(TokenType.Terminal, lexeme);
}
