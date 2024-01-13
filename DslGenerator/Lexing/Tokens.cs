namespace RhoMicro.CodeAnalysis.DslGenerator.Lexing;

#if DSL_GENERATOR
[IncludeFile]
#endif
static class Tokens
{
    public static Token Equal { get; } = new(TokenType.Equal, Lexemes.Equal);
    public static Token Period { get; } = new(TokenType.Period, Lexemes.Period);
    public static Token Dash { get; } = new(TokenType.Dash, Lexemes.Dash);
    public static Token Alternative { get; } = new(TokenType.Slash, Lexemes.Alternative);
    public static Token IncrementalAlternative { get; } = new(TokenType.SlashEqual, Lexemes.IncrementalAlternative);
    public static Token GroupOpen { get; } = new(TokenType.ParenLeft, Lexemes.GroupOpen);
    public static Token GroupClose { get; } = new(TokenType.ParenRight, Lexemes.GroupClose);
    public static Token VariableRepetition { get; } = new(TokenType.Star, Lexemes.VariableRepetition);
    public static Token OptionalSequenceOpen { get; } = new(TokenType.BracketLeft, Lexemes.OptionalSequenceOpen);
    public static Token OptionalSequenceClose { get; } = new(TokenType.BracketRight, Lexemes.OptionalSequenceClose);
    public static Token Semicolon { get; } = new(TokenType.Semicolon, Lexemes.Semicolon);
    public static Token NewLine { get; } = new(TokenType.NewLine, Lexemes.NewLine);
    public static Token Eof { get; } = new(TokenType.Eof, Lexemes.Eof);
}