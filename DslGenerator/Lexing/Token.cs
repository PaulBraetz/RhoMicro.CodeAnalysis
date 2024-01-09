namespace RhoMicro.CodeAnalysis.DslGenerator.Lexing;

using Microsoft.CodeAnalysis;

readonly record struct Token(TokenType Type, Lexeme Lexeme, Location Location)
{
    public Token(TokenType type, Lexeme lexeme) : this(type, lexeme, Location.None) { }
    public override String ToString() => $"[{Type}:{Lexeme.ToEscapedString()}]";
    public static Token CreateUnknown(Lexeme lexeme) => new(TokenType.Unknown, lexeme);
    public static Token CreateName(Lexeme lexeme) => new(TokenType.Name, lexeme);
    public static Token CreateWhitespace(Lexeme lexeme) => new(TokenType.Whitespace, lexeme);
    public static Token CreateSpecificRepetition(Lexeme lexeme) => new(TokenType.Number, lexeme);
    public static Token CreateComment(Lexeme lexeme) => new(TokenType.Comment, lexeme);
    public static Token CreateTerminal(Lexeme lexeme) => new(TokenType.Terminal, lexeme);
}
