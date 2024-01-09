#pragma warning disable IDE2003 // Blank line required between block and subsequent statement
#pragma warning disable CA1822 // Mark members as static
namespace RhoMicro.CodeAnalysis.DslGenerator.Lexing;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

using System.Collections.Immutable;
using RhoMicro.CodeAnalysis.DslGenerator.Analysis;

using static Lexemes;
using static RhoMicro.CodeAnalysis.DslGenerator.Analysis.DiagnosticDescriptors;

partial class Tokenizer
{
    [UnionType(typeof(Token))]
    [UnionType(typeof(TokenType))]
    readonly partial struct TokenOrType;

    public TokenizeResult Tokenize(SourceText sourceText, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var tokensBuilder = ImmutableArray.CreateBuilder<Token>();
        var diagnostics = new DiagnosticsCollection();
        var source = sourceText.ToString(cancellationToken);
        var isUnknown = false;
        var (start, current, line, character) = (0, 0, 0, 0);

        while(!isAtEnd())
        {
            cancellationToken.ThrowIfCancellationRequested();
            scanToken();
        }

        addToken(Tokens.Eof);

        var tokens = tokensBuilder.ToImmutable();
        return new(tokens, diagnostics);

        void scanToken()
        {
            var c = advance();
            switch(c)
            {
                case Equal:
                    addToken(TokenType.Equal);
                    break;
                case Alternative:
                    //check for incremental alternative "/="
                    var type = match(Equal) ?
                        TokenType.SlashEqual :
                        TokenType.Slash;
                    addToken(type);
                    break;
                case GroupOpen:
                    addToken(TokenType.ParenLeft);
                    break;
                case GroupClose:
                    addToken(TokenType.ParenRight);
                    break;
                case VariableRepetition:
                    addToken(TokenType.Star);
                    break;
                case OptionalSequenceOpen:
                    addToken(TokenType.BracketLeft);
                    break;
                case OptionalSequenceClose:
                    addToken(TokenType.BracketRight);
                    break;
                case Semicolon:
                    addToken(TokenType.Semicolon);
                    break;
                case Hash:
                    comment();
                    break;
                case Quote:
                    terminal();
                    break;
                case NewLine:
                    addNewLine();
                    break;
                case CarriageReturn:
                    closeUnknown();
                    if(lookAhead() == NewLine)
                        advancePure();
                    addNewLine();
                    break;
                case Space:
                    consumeWhitespace(Space);
                    break;
                case Tab:
                    consumeWhitespace(Tab);
                    break;
                default:
                    if(isAlpha(c))
                    {
                        name();
                    } else if(isDigit(c))
                    {
                        specificRepetition();
                    } else
                    {
                        openUnknown();
                    }

                    break;
            }

            void addNewLine()
            {
                closeUnknown();
                line++;
                character = 0;
                addToken(TokenType.NewLine);
            }
        }
        Boolean isAtEnd(Int32 lookaheadOffset = 0) => current + lookaheadOffset >= source!.Length;
        Char advance()
        {
            advancePure();
            return source![current - 1];
        }
        void specificRepetition()
        {
            closeUnknown();

            while(isDigit(lookAhead()))
                advancePure();
            addToken(TokenType.Number);
        }
        void advancePure()
        {
            character++;
            current++;
        }
        void regressPure()
        {
            character--;
            current--;
        }
        void openUnknown() => isUnknown = true;
        void closeUnknown()
        {
            if(isUnknown)
            {
                if(!isAtEnd())
                    regressPure();
                isUnknown = false;
                addToken(TokenType.Unknown);
                diagnostics!.Add(UnexpectedCharacter, getLocation());
                if(!isAtEnd())
                    advancePure();
            }
        }
        void resetLexemeStart() => start = current;
        void addToken(TokenOrType tokenOrType)
        {
            closeUnknown();

            var token = tokenOrType.Match(
                token => token,
                type => new Token(type, getLexeme(), getLocation()));
            tokensBuilder!.Add(token);
            resetLexemeStart();
        }
        Boolean isDigit(Char? c) => c is not null and >= '0' and <= '9';
        Boolean isAlpha(Char? c) => c is not null and (>= 'a' and <= 'z' or >= 'A' and <= 'Z' or '_');
        Lexeme getLexeme() => new StringSlice(source!, start, current - start);
        Char? lookAhead(Int32 lookAheadOffset = 0) => current + lookAheadOffset >= source!.Length ? null : source![current + lookAheadOffset];
        Char? lookBehind(Int32 lookBehindOffset = 0) => current - lookBehindOffset < 1 ? null : source![current - 1 - lookBehindOffset];
        Location getLocation() =>
            sourceText.MatchLocation(
                additionalText =>
                {
                    var sourceText = additionalText.GetText(cancellationToken);
                    if(sourceText == null)
                    {
                        return Location.None;
                    }

                    var path = additionalText.Path;
                    var lineSpan = new LinePositionSpan(
                        new LinePosition(line, character),
                        new LinePosition(line, character));
                    var textSpan = sourceText.Lines.GetTextSpan(lineSpan);
                    var location = Location.Create(path, textSpan, lineSpan);
                    return location;
                });
        Boolean match(Char expected)
        {
            if(isAtEnd() || source![current] != expected)
                return false;
            current++;
            return true;
        }
        Boolean isAtNewLine() =>
            lookAhead() switch
            {
                NewLine => true,
                CarriageReturn => true,
                _ => false
            };
        void comment()
        {
            addToken(TokenType.Hash);
            advancePure(); //consume hash

            if(isAtNewLine() || isAtEnd())
                return;

            while(!isAtNewLine() && !isAtEnd(1))
                advancePure();

            if(!isAtNewLine())
                advancePure(); //consume last comment char

            addToken(TokenType.Comment);
        }
        void consumeWhitespace(Char expected)
        {
            closeUnknown();

            while(lookAhead() == expected)
                advancePure();

            addToken(TokenType.Whitespace);
        }
        void terminal()
        {
            addToken(TokenType.Quote);
            var containsCharacters = false;
            while((lookAhead() != Quote || lookBehind() == Escape) && !isAtEnd())
            {
                if(lookAhead() == NewLine)
                {
                    line++;
                }

                advancePure();
                containsCharacters = true;
            }

            if(containsCharacters)
            {
                addToken(TokenType.Terminal);
            }

            if(isAtEnd())
            {
                diagnostics.Add(UnterminatedTerminal, getLocation(), getLexeme());
                return;
            }

            //consume terminating quote
            advancePure();
            addToken(TokenType.Quote);
        }
        void name()
        {
            closeUnknown();

            while(isAlpha(lookAhead()))
                advancePure();

            addToken(TokenType.Name);
        }
    }
}
