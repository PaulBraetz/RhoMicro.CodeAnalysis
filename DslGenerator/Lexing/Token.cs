﻿namespace RhoMicro.CodeAnalysis.DslGenerator.Lexing;

#if DSL_GENERATOR
[IncludeFile]
#endif
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
    public Boolean Equals(Token other) => Type == other.Type && Lexeme == other.Lexeme;
    public override Int32 GetHashCode() => (Type, Lexeme).GetHashCode();
}
readonly record struct Token<T>(T Type, Lexeme Lexeme, Location Location)
{
    public Token(T type, Lexeme lexeme) : this(type, lexeme, Location.None) { }
    public override String ToString() => $"[{Type}:{Lexeme.ToEscapedString()}]";
    public Boolean Equals(Token<T> other) => (Type, Lexeme).Equals((other.Type, other.Lexeme));
    public override Int32 GetHashCode() => (Type, Lexeme).GetHashCode();
}