#pragma warning disable CA1861 // Avoid constant arrays as arguments
namespace DslGenerator.Tests;

using RhoMicro.CodeAnalysis.DslGenerator.Analysis;
using RhoMicro.CodeAnalysis.DslGenerator.Lexing;

using System.Collections.Immutable;
using System.Diagnostics;

public class TokenizerTests
{
#pragma warning disable RS1035 // Do not use APIs banned for analyzers
    public static Object[][] TokenizerData => [
        ["name=name", new[] { "Name:name", "Equal:=", "Name:name" }, Array.Empty<Int32>()],
        ["name=\"\";", new[] { "Name:name", "Equal:=", "Terminal:", "Semicolon:;" }, Array.Empty<Int32>()],
        ["name=\"a\"-\"z\";", new[] { "Name:name", "Equal:=", "Terminal:a", "Dash:-", "Terminal:z", "Semicolon:;" }, Array.Empty<Int32>()],
        ["name", new[] { "Name:name" }, Array.Empty<Int32>()],
        ["=", new[] { "Equal:=" }, Array.Empty<Int32>()],
        [".", new[] { "Period:." }, Array.Empty<Int32>()],
        ["TerminalValue = \"\\\"\" . \"\\\"\";",
            new[] { "Name:TerminalValue", "Whitespace: ", "Equal:=", "Whitespace: ", "Terminal:\\\"", "Whitespace: ", "Period:.", "Whitespace: ", "Terminal:\\\"", "Semicolon:;" },
            Array.Empty<Int32>()],
        ["-", new[] { "Dash:-" }, Array.Empty<Int32>()],
        ["\"a\"-\"z\"", new[] { "Terminal:a", "Dash:-", "Terminal:z" }, Array.Empty<Int32>()],
        ["\"ab\"-\"z\"", new[] { "Terminal:ab", "Dash:-", "Terminal:z" }, Array.Empty<Int32>()],
        ["     ", new[] { "Whitespace:     " }, Array.Empty<Int32>()],
        [" ", new[] { "Whitespace: " }, Array.Empty<Int32>()],
        ["\t", new[] { "Whitespace:\t" }, Array.Empty<Int32>()],
        ["\t\t\t", new[] { "Whitespace:\t\t\t" }, Array.Empty<Int32>()],
        ["\n", new[] { "NewLine:\n" }, Array.Empty<Int32>()],
        ["\n\n\n", new[] { "NewLine:\n", "NewLine:\n", "NewLine:\n" }, Array.Empty<Int32>()],
        ["\r", new[] { "NewLine:\r" }, Array.Empty<Int32>()],
        ["\r\r\r", new[] { "NewLine:\r", "NewLine:\r", "NewLine:\r" }, Array.Empty<Int32>()],
        ["\r\n", new[] { "NewLine:\r\n" }, Array.Empty<Int32>()],
        ["\r\n\r\n\r\n", new[] { "NewLine:\r\n", "NewLine:\r\n", "NewLine:\r\n" }, Array.Empty<Int32>()],
        ["\r\n\n\r\n", new[] { "NewLine:\r\n", "NewLine:\n", "NewLine:\r\n" }, Array.Empty<Int32>()],
        [";", new[] { "Semicolon:;" }, Array.Empty<Int32>()],
        ["\"", Array.Empty<String>(), Array.Empty<Int32>()],
        ["/", new[] { "Slash:/" }, Array.Empty<Int32>()],
        ["#", Array.Empty<String>(), Array.Empty<Int32>()],
        ["# this is a comment; 24224 ßßßß", new[] { "Comment: this is a comment; 24224 ßßßß" }, Array.Empty<Int32>()],
        ["/=", new[] { "SlashEqual:/=" }, Array.Empty<Int32>()],
        ["(", new[] { "ParenLeft:(" }, Array.Empty<Int32>()],
        [")", new[] { "ParenRight:)" }, Array.Empty<Int32>()],
        ["[", new[] { "BracketLeft:[" }, Array.Empty<Int32>()],
        ["]", new[] { "BracketRight:]" }, Array.Empty<Int32>()],
        ["32rule", new[] { "Number:32", "Name:rule" }, Array.Empty<Int32>()],
        ["&33+", new[] { "Unknown:&", "Number:33", "Unknown:+" }, Array.Empty<Int32>()],
        ["%%%", new[] { "Unknown:%%%" }, new[] { 1 }],
        ["\"TerminalValue\"", new[] { "Terminal:TerminalValue" }, Array.Empty<Int32>()],
        ["\"TerminalValue", new[] { "Terminal:TerminalValue" }, new[] { 2 }],
        ["\"TerminalValue;rule=somename;", new[] { "Terminal:TerminalValue;rule=somename;" }, new[] { 2 }],
        ["\"TerminalValue;ru\\\"le=somename;\"", new[] { "Terminal:TerminalValue;ru\\\"le=somename;" }, Array.Empty<Int32>()],
        [       """
                testname = ["is this the 1 legal value?"] / # here is a comment
                    *"no, it's not :)"; # and another comment
                
                """,
            new[] { "Name:testname", "Whitespace: ", "Equal:=", "Whitespace: ",
                             "BracketLeft:[", "Terminal:is this the 1 legal value?", "BracketRight:]",
                             "Whitespace: ","Slash:/","Whitespace: ", "Comment: here is a comment", $"NewLine:{Environment.NewLine}",
                             "Whitespace:    ","Star:*","Terminal:no, it's not :)" ,
                             "Semicolon:;", "Whitespace: ", "Comment: and another comment", $"NewLine:{Environment.NewLine}",
                             },
            Array.Empty<Int32>()],
        ["a=b;a/=c;", new[] { "Name:a", "Equal:=", "Name:b", "Semicolon:;", "Name:a", "SlashEqual:/=", "Name:c", "Semicolon:;" }, Array.Empty<Int32>()]
    ];
#pragma warning restore RS1035 // Do not use APIs banned for analyzers

    [Theory(Timeout = 5000)]
    [MemberData(nameof(TokenizerData))]
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