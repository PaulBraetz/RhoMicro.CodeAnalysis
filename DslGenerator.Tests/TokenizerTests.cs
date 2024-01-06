namespace DslGenerator.Tests;

using RhoMicro.CodeAnalysis.DslGenerator.Lexing;

using System.Collections.Immutable;

public class TokenizerTests
{
    [Theory(Timeout = 5000)]
    [InlineData("name=name", new[] { "Name:name", "Equal:=", "Name:name" })]
    [InlineData("name", new[] { "Name:name" })]
    [InlineData("=", new[] { "Equal:=" })]
    [InlineData(";", new[] { "Semicolon:;" })]
    [InlineData("\"TerminalValue\"", new[] { "Quote:\"", "Terminal:TerminalValue", "Quote:\"" })]
    public void Test1(String source, String[] rawTokens)
    {
        var tokens = rawTokens.Select(t => t.Split(':')).Select(t => (Type: Enum.Parse<TokenType>(t[0]), Lexeme: (Lexeme)t[1]));
        var actualTokens = new Tokenizer([]).Tokenize(source, default);
        var expectedTokens = tokens.Select(t => new Token(t.Type, t.Lexeme));
        var condition = actualTokens.SequenceEqual(expectedTokens);
        Assert.True(condition);
    }
}