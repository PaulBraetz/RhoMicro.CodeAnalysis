#pragma warning disable CA1861 // Avoid constant arrays as arguments
namespace DslGenerator.Tests;

using RhoMicro.CodeAnalysis.DslGenerator.Analysis;
using RhoMicro.CodeAnalysis.DslGenerator.Lexing;

using System.Collections.Immutable;
using System.Diagnostics;

public class TokenizerTests
{
    [Theory(Timeout = 5000)]
    [InlineData("name=name", new[] { "Name:name", "Equal:=", "Name:name" }, new Int32[] { })]
    [InlineData("name=\"\";", new[] { "Name:name", "Equal:=", "Terminal:", "Semicolon:;" }, new Int32[] { })]
    [InlineData("name=\"a\"-\"z\";", new[] { "Name:name", "Equal:=", "Terminal:a", "Dash:-", "Terminal:z", "Semicolon:;" }, new Int32[] { })]
    [InlineData("name", new[] { "Name:name" }, new Int32[] { })]
    [InlineData("=", new[] { "Equal:=" }, new Int32[] { })]
    [InlineData(".", new[] { "Period:." }, new Int32[] { })]
    [InlineData("TerminalValue = \"\\\"\" . \"\\\"\";",
        new[] { "Name:TerminalValue", "Whitespace: ", "Equal:=", "Whitespace: ", "Terminal:\\\"", "Whitespace: ", "Period:.", "Whitespace: ", "Terminal:\\\"","Semicolon:;" }, new Int32[] { })]
    [InlineData("-", new[] { "Dash:-" }, new Int32[] { })]
    [InlineData("\"a\"-\"z\"", new[] { "Terminal:a", "Dash:-", "Terminal:z" }, new Int32[] { })]
    [InlineData("\"ab\"-\"z\"", new[] { "Terminal:ab", "Dash:-", "Terminal:z" }, new Int32[] { })]
    [InlineData("     ", new[] { "Whitespace:     " }, new Int32[] { })]
    [InlineData(" ", new[] { "Whitespace: " }, new Int32[] { })]
    [InlineData("\t", new[] { "Whitespace:\t" }, new Int32[] { })]
    [InlineData("\t\t\t", new[] { "Whitespace:\t\t\t" }, new Int32[] { })]
    [InlineData("\n", new[] { "NewLine:\n" }, new Int32[] { })]
    [InlineData("\n\n\n", new[] { "NewLine:\n", "NewLine:\n", "NewLine:\n" }, new Int32[] { })]
    [InlineData("\r", new[] { "NewLine:\r" }, new Int32[] { })]
    [InlineData("\r\r\r", new[] { "NewLine:\r", "NewLine:\r", "NewLine:\r" }, new Int32[] { })]
    [InlineData("\r\n", new[] { "NewLine:\r\n" }, new Int32[] { })]
    [InlineData("\r\n\r\n\r\n", new[] { "NewLine:\r\n", "NewLine:\r\n", "NewLine:\r\n" }, new Int32[] { })]
    [InlineData("\r\n\n\r\n", new[] { "NewLine:\r\n", "NewLine:\n", "NewLine:\r\n" }, new Int32[] { })]
    [InlineData(";", new[] { "Semicolon:;" }, new Int32[] { })]
    [InlineData("\"", new String[] { }, new Int32[] { })]
    [InlineData("/", new[] { "Slash:/" }, new Int32[] { })]
    [InlineData("#", new String[] { }, new Int32[] { })]
    [InlineData("# this is a comment; 24224 ßßßß", new[] { "Comment: this is a comment; 24224 ßßßß" }, new Int32[] { })]
    [InlineData("/=", new[] { "SlashEqual:/=" }, new Int32[] { })]
    [InlineData("(", new[] { "ParenLeft:(" }, new Int32[] { })]
    [InlineData(")", new[] { "ParenRight:)" }, new Int32[] { })]
    [InlineData("[", new[] { "BracketLeft:[" }, new Int32[] { })]
    [InlineData("]", new[] { "BracketRight:]" }, new Int32[] { })]
    [InlineData("32rule", new[] { "Number:32", "Name:rule" }, new Int32[] { })]
    [InlineData("&33+", new[] { "Unknown:&", "Number:33", "Unknown:+" }, new Int32[] { })]
    [InlineData("%%%", new[] { "Unknown:%%%" }, new[] { 1 })]
    [InlineData("\"TerminalValue\"", new[] { "Terminal:TerminalValue" }, new Int32[] { })]
    [InlineData("\"TerminalValue", new[] { "Terminal:TerminalValue" }, new[] { 2 })]
    [InlineData("\"TerminalValue;rule=somename;", new[] { "Terminal:TerminalValue;rule=somename;" }, new[] { 2 })]
    [InlineData("\"TerminalValue;ru\\\"le=somename;\"", new[] { "Terminal:TerminalValue;ru\\\"le=somename;" }, new Int32[] { })]
    [InlineData("""
                testname = ["is this the 1 legal value?"] / # here is a comment
                    *"no, it's not :)"; # and another comment
                
                """, new[] { "Name:testname", "Whitespace: ", "Equal:=", "Whitespace: ",
                             "BracketLeft:[", "Terminal:is this the 1 legal value?", "BracketRight:]",
                             "Whitespace: ","Slash:/","Whitespace: ", "Comment: here is a comment", "NewLine:\r\n",
                             "Whitespace:    ","Star:*","Terminal:no, it's not :)" ,
                             "Semicolon:;", "Whitespace: ", "Comment: and another comment", "NewLine:\r\n",
                             }, new Int32[] { })]
    [InlineData("a=b;a/=c;", new[] { "Name:a", "Equal:=", "Name:b", "Semicolon:;", "Name:a", "SlashEqual:/=", "Name:c", "Semicolon:;" }, new Int32[] { })]
    public void TokenizesCorrectly(String source, String[] rawTokens, Int32[] expectedDiagnosticIds)
    {
        //Arrange
        var expectedTokens = rawTokens.Select(t => t.Split(':', 2))
            .Select(t => (Type: Enum.Parse<TokenType>(t[0]), Lexeme: (Lexeme)String.Concat(t[1..])))
            .Select(t => new Token(t.Type, t.Lexeme))
            .Append(new Token(TokenType.Eof, Lexeme.Empty))
            .ToImmutableArray();
        var tokenizer = new Tokenizer();

        //Act
        var (actualTokens, actualDiagnostics) = tokenizer.Tokenize(source, default, String.Empty);

        //Assert
        try
        {
            VerifyTokens(expectedTokens, actualTokens);
            VerifyDiagnostics(expectedDiagnosticIds, actualDiagnostics);
        } catch
        {
            Debugger.Break();
            _ = Tokenizer.Instance.Tokenize(source, default);
            throw;
        }
    }

    private static void VerifyDiagnostics(Int32[] expectedDiagnosticIds, DiagnosticsCollection actualDiagnostics)
    {
        var actualDiagnosticIdCounts = actualDiagnostics.GroupBy(d => d.Descriptor.Id)
                    .Select(g => (id: g.Key, count: g.Count()))
                    .ToDictionary(t => t.id, t => t.count);
        foreach(var (id, expectedCount) in expectedDiagnosticIds.GroupBy(id => id).Select(g => (id: g.Key, count: g.Count())))
        {
            var actualCount = actualDiagnosticIdCounts.TryGetValue(id, out var c) ? c : -1;
            Assert.Equal(expectedCount, actualCount);
        }
    }

    private static void VerifyTokens(IReadOnlyList<Token> expectedTokens, IReadOnlyList<Token> actualTokens)
    {
        for(var i = 0; i < expectedTokens.Count; i++)
        {
            var expectedToken = expectedTokens[i];
            if(actualTokens.Count <= i)
                Assert.Fail($"Not enough tokens produced; missing {expectedToken}");

            var actualToken = actualTokens[i];
            Assert.Equal(expectedToken, actualToken);
        }
    }
}