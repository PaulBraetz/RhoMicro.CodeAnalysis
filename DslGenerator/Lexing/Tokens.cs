namespace RhoMicro.CodeAnalysis.DslGenerator.Lexing;

static class Tokens
{
    public static Token Equal { get; } = new(TokenType.Equal, Lexemes.Equal);
    public static Token Alternative { get; } = new(TokenType.Alternative, Lexemes.Alternative);
    public static Token IncrementalAlternative { get; } = new(TokenType.IncrementalAlternative, Lexemes.IncrementalAlternative);
    public static Token GroupOpen { get; } = new(TokenType.GroupOpen, Lexemes.GroupOpen);
    public static Token GroupClose { get; } = new(TokenType.GroupClose, Lexemes.GroupClose);
    public static Token VariableRepetition { get; } = new(TokenType.VariableRepetition, Lexemes.VariableRepetition);
    public static Token OptionalSequenceOpen { get; } = new(TokenType.OptionalSequenceOpen, Lexemes.OptionalSequenceOpen);
    public static Token OptionalSequenceClose { get; } = new(TokenType.OptionalSequenceClose, Lexemes.OptionalSequenceClose);
    public static Token Hash { get; } = new(TokenType.Hash, Lexemes.Hash);
    public static Token Semicolon { get; } = new(TokenType.Semicolon, Lexemes.Semicolon);
    public static Token NewLine { get; } = new(TokenType.NewLine, Lexemes.NewLine);
    public static Token Quote { get; } = new(TokenType.Quote, Lexemes.Quote);
    public static Token Eof { get; } = new(TokenType.Eof, Lexemes.Eof);
}