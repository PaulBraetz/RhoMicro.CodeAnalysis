namespace RhoMicro.CodeAnalysis.DslGenerator.Lexing;

using static RhoMicro.CodeAnalysis.DslGenerator.Analysis.DiagnosticDescriptors;
using RhoMicro.CodeAnalysis.DslGenerator.Grammar;
using RhoMicro.CodeAnalysis.DslGenerator.Analysis;

using static Lexemes;

#if DSL_GENERATOR
[IncludeFile]
#endif
partial class Tokenizer
{
    [UnionType(typeof(Token))]
    [UnionType(typeof(TokenType))]
    readonly partial struct TokenOrType;
    public static Tokenizer Instance { get; } = new();
    public TokenizeResult Tokenize(SourceText sourceText, CancellationToken cancellationToken
#if DSL_GENERATOR
        , String filePath = ""
#endif
        )
    {
        cancellationToken.ThrowIfCancellationRequested();

        var tokens = new List<Token>();
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

        return new(tokens, diagnostics);

        void scanToken()
        {
            var c = advance();
            switch(c)
            {
                case Equal:
                    addToken(TokenType.Equal);
                    break;
                case Dash:
                    addToken(TokenType.Dash);
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
            tokens!.Add(token);
            resetLexemeStart();
        }

        void discardToken()
        {
            closeUnknown();
            resetLexemeStart();
        }

        Boolean isDigit(Char? c) => c is not null and >= '0' and <= '9';

        Boolean isAlpha(Char? c) => c is not null && Utils.IsValidNameChar(c.Value);

        Lexeme getLexeme() => new StringSlice(source!, start, current - start);

        Char? lookAhead(Int32 lookAheadOffset = 0) => current + lookAheadOffset >= source!.Length ? null : source![current + lookAheadOffset];

        Char? lookBehind(Int32 lookBehindOffset = 0) => current - lookBehindOffset < 1 ? null : source![current - 1 - lookBehindOffset];

        Location getLocation() => Location.Create(
            line,
            character,
            current
#if DSL_GENERATOR
            , filePath
#endif
            );

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
            discardToken(); //discard hash token
            advancePure(); //consume hash

            if(isAtNewLine() || isAtEnd())
                return;

            while(!isAtNewLine() && !isAtEnd(lookaheadOffset: 1))
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
            discardToken(); //discard quote token
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

            //add empty token
            if(!containsCharacters)
            {
                addToken(TokenType.Terminal);
            }

            //consume terminating quote
            advancePure();
            discardToken(); //discard quote token
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
