namespace DslGenerator.Tests;

using RhoMicro.CodeAnalysis.DslGenerator.Lexing;

using System.Collections.Immutable;

public class TokenizerTests
{
    [Theory(Timeout = 5000)]
    [InlineData("name=name", new[] { "Name:name", "Equal:=", "Name:name" })]
    [InlineData("name", new[] { "Name:name" })]
    [InlineData("=", new[] { "Equal:=" })]
    [InlineData("     ", new[] { "Whitespace:     " })]
    [InlineData(" ", new[] { "Whitespace: " })]
    [InlineData("\t", new[] { "Whitespace:\t" })]
    [InlineData("\t\t\t", new[] { "Whitespace:\t\t\t" })]
    [InlineData("\n", new[] { "NewLine:\n" })]
    [InlineData("\n\n\n", new[] { "NewLine:\n", "NewLine:\n", "NewLine:\n" })]
    [InlineData("\r", new[] { "NewLine:\r" })]
    [InlineData("\r\r\r", new[] { "NewLine:\r", "NewLine:\r", "NewLine:\r" })]
    [InlineData("\r\n", new[] { "NewLine:\r\n" })]
    [InlineData("\r\n\r\n\r\n", new[] { "NewLine:\r\n", "NewLine:\r\n", "NewLine:\r\n" })]
    [InlineData("\r\n\n\r\n", new[] { "NewLine:\r\n", "NewLine:\n", "NewLine:\r\n" })]
    [InlineData(";", new[] { "Semicolon:;" })]
    [InlineData("\"", new[] { "Quote:\"" })]
    [InlineData("/", new[] { "Alternative:/" })]
    [InlineData("#", new[] { "Hash:#" })]
    [InlineData("# this is a comment; 24224 ßßßß", new[] { "Hash:#", "Comment: this is a comment; 24224 ßßßß" })]
    [InlineData("=/", new[] { "IncrementalAlternative:=/" })]
    [InlineData("(", new[] { "GroupOpen:(" })]
    [InlineData(")", new[] { "GroupClose:)" })]
    [InlineData("[", new[] { "OptionalSequenceOpen:[" })]
    [InlineData("]", new[] { "OptionalSequenceClose:]" })]
    [InlineData("32rule", new[] { "SpecificRepetition:32", "Name:rule" })]
    [InlineData("ß33§", new[] { "Unknown:ß", "SpecificRepetition:33", "Unknown:§" })]
    [InlineData("\"TerminalValue\"", new[] { "Quote:\"", "Terminal:TerminalValue", "Quote:\"" })]
    [InlineData("""
                testname = ["is this the 1 legal value?"] / # here is a comment
                    *"no, it's not :)"; # and another comment
                
                """, new[] { "Name:testname", "Whitespace: ", "Equal:=", "Whitespace: ",
                             "OptionalSequenceOpen:[","Quote:\"","Terminal:is this the 1 legal value?","Quote:\"","OptionalSequenceClose:]",
                             "Whitespace: ","Alternative:/","Whitespace: ","Hash:#", "Comment: here is a comment", "NewLine:\r\n",
                             "Whitespace:    ","VariableRepetition:*", "Quote:\"","Terminal:no, it's not :)" , "Quote:\"",
                             "Semicolon:;", "Whitespace: ","Hash:#", "Comment: and another comment", "NewLine:\r\n",
                             })]
    public void Test1(String source, String[] rawTokens)
    {
        //Arrange
        var expectedTokens = rawTokens.Select(t => t.Split(':', 2))
            .Select(t => (Type: Enum.Parse<TokenType>(t[0]), Lexeme: (Lexeme)String.Concat(t[1..])))
            .Select(t => new Token(t.Type, t.Lexeme))
            .Append(new Token(TokenType.Eof, Lexeme.Empty))
            .ToImmutableArray();
        var tokenizer = new Tokenizer([]);

        //Act
        var actualTokens = tokenizer.Tokenize(source, default);

        //Assert
        for(var i = 0; i < expectedTokens.Length; i++)
        {
            var expectedToken = expectedTokens[i];
            if(actualTokens.Length <= i)
                Assert.Fail($"Not enough tokens produced; missing {expectedToken}");

            var actualToken = actualTokens[i];
            Assert.Equal(expectedToken, actualToken);
        }
    }
}