namespace DslGenerator.Tests;

using RhoMicro.CodeAnalysis.DslGenerator.Analysis;
using RhoMicro.CodeAnalysis.DslGenerator.Lexing;

using System.Collections.Immutable;

public class TokenizerTests
{
    [Theory(Timeout = 5000)]
    [InlineData("name=name", new[] { "Name:name", "Equal:=", "Name:name" }, new String[] { })]
    [InlineData("name", new[] { "Name:name" }, new String[] { })]
    [InlineData("=", new[] { "Equal:=" }, new String[] { })]
    [InlineData("     ", new[] { "Whitespace:     " }, new String[] { })]
    [InlineData(" ", new[] { "Whitespace: " }, new String[] { })]
    [InlineData("\t", new[] { "Whitespace:\t" }, new String[] { })]
    [InlineData("\t\t\t", new[] { "Whitespace:\t\t\t" }, new String[] { })]
    [InlineData("\n", new[] { "NewLine:\n" }, new String[] { })]
    [InlineData("\n\n\n", new[] { "NewLine:\n", "NewLine:\n", "NewLine:\n" }, new String[] { })]
    [InlineData("\r", new[] { "NewLine:\r" }, new String[] { })]
    [InlineData("\r\r\r", new[] { "NewLine:\r", "NewLine:\r", "NewLine:\r" }, new String[] { })]
    [InlineData("\r\n", new[] { "NewLine:\r\n" }, new String[] { })]
    [InlineData("\r\n\r\n\r\n", new[] { "NewLine:\r\n", "NewLine:\r\n", "NewLine:\r\n" }, new String[] { })]
    [InlineData("\r\n\n\r\n", new[] { "NewLine:\r\n", "NewLine:\n", "NewLine:\r\n" }, new String[] { })]
    [InlineData(";", new[] { "Semicolon:;" }, new String[] { })]
    [InlineData("\"", new[] { "Quote:\"" }, new String[] { })]
    [InlineData("/", new[] { "Slash:/" }, new String[] { })]
    [InlineData("#", new[] { "Hash:#" }, new String[] { })]
    [InlineData("# this is a comment; 24224 ßßßß", new[] { "Hash:#", "Comment: this is a comment; 24224 ßßßß" }, new String[] { })]
    [InlineData("/=", new[] { "SlashEqual:/=" }, new String[] { })]
    [InlineData("(", new[] { "ParenLeft:(" }, new String[] { })]
    [InlineData(")", new[] { "ParenRight:)" }, new String[] { })]
    [InlineData("[", new[] { "BracketLeft:[" }, new String[] { })]
    [InlineData("]", new[] { "BracketRight:]" }, new String[] { })]
    [InlineData("32rule", new[] { "Number:32", "Name:rule" }, new String[] { })]
    [InlineData("-33+", new[] { "Unknown:-", "Number:33", "Unknown:+" }, new String[] { })]
    [InlineData("---", new[] { "Unknown:---" }, new[] { "DSLG0001" })]
    [InlineData("\"TerminalValue\"", new[] { "Quote:\"", "Terminal:TerminalValue", "Quote:\"" }, new String[] { })]
    [InlineData("\"TerminalValue", new[] { "Quote:\"", "Terminal:TerminalValue" }, new[] { "DSLG0002" })]
    [InlineData("\"TerminalValue;rule=somename;", new[] { "Quote:\"", "Terminal:TerminalValue;rule=somename;" }, new[] { "DSLG0002" })]
    [InlineData("\"TerminalValue;ru\\\"le=somename;\"", new[] { "Quote:\"", "Terminal:TerminalValue;ru\\\"le=somename;", "Quote:\"" }, new String[] { })]
    [InlineData("""
                testname = ["is this the 1 legal value?"] / # here is a comment
                    *"no, it's not :)"; # and another comment
                
                """, new[] { "Name:testname", "Whitespace: ", "Equal:=", "Whitespace: ",
                             "BracketLeft:[","Quote:\"","Terminal:is this the 1 legal value?","Quote:\"","BracketRight:]",
                             "Whitespace: ","Slash:/","Whitespace: ","Hash:#", "Comment: here is a comment", "NewLine:\r\n",
                             "Whitespace:    ","Star:*", "Quote:\"","Terminal:no, it's not :)" , "Quote:\"",
                             "Semicolon:;", "Whitespace: ","Hash:#", "Comment: and another comment", "NewLine:\r\n",
                             }, new String[] { })]
    [InlineData("a=b;a/=c;", new[] { "Name:a", "Equal:=", "Name:b", "Semicolon:;", "Name:a", "SlashEqual:/=", "Name:c", "Semicolon:;" }, new String[] { })]
    public void TokenizesCorrectly(String source, String[] rawTokens, String[] expectedDiagnosticIds)
    {
        //Arrange
        var expectedTokens = rawTokens.Select(t => t.Split(':', 2))
            .Select(t => (Type: Enum.Parse<TokenType>(t[0]), Lexeme: (Lexeme)String.Concat(t[1..])))
            .Select(t => new Token(t.Type, t.Lexeme))
            .Append(new Token(TokenType.Eof, Lexeme.Empty))
            .ToImmutableArray();
        var tokenizer = new Tokenizer();

        //Act
        var (actualTokens, actualDiagnostics) = tokenizer.Tokenize(source, default);

        //Assert
        VerifyTokens(expectedTokens, actualTokens);
        VerifyDiagnostics(expectedDiagnosticIds, actualDiagnostics);
    }

    private static void VerifyDiagnostics(String[] expectedDiagnosticIds, DiagnosticsCollection actualDiagnostics)
    {
        var actualDiagnosticIdCounts = actualDiagnostics.GroupBy(d => d.Id)
                    .Select(g => (id: g.Key, count: g.Count()))
                    .ToDictionary(t => t.id, t => t.count);
        foreach(var (id, expectedCount) in expectedDiagnosticIds.GroupBy(id => id).Select(g => (id: g.Key, count: g.Count())))
        {
            var actualCount = actualDiagnosticIdCounts.TryGetValue(id, out var c) ? c : -1;
            Assert.Equal(expectedCount, actualCount);
        }
    }

    private static void VerifyTokens(ImmutableArray<Token> expectedTokens, ImmutableArray<Token> actualTokens)
    {
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