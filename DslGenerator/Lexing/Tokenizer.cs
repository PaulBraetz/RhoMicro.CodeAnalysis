﻿namespace RhoMicro.CodeAnalysis.DslGenerator.Lexing;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

using System.Collections.Immutable;
using System.Runtime.CompilerServices;

using static Lexemes;
using static DiagnosticDescriptors;

sealed partial class Tokenizer(DiagnosticsCollection diagnostics)
{
    [UnionType(typeof(Token))]
    [UnionType(typeof(TokenType))]
    readonly partial struct TokenOrType;
    private readonly DiagnosticsCollection _diagnostics = diagnostics;

    public ImmutableArray<Token> Tokenize(SourceText sourceText, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        //TODO: optimize waste
        var tokens = ImmutableArray.CreateBuilder<Token>();
        var source = sourceText.ToString(cancellationToken);
        var (start, current, line, character) = (0, 0, 0, 0);

        while(!isAtEnd())
        {
            cancellationToken.ThrowIfCancellationRequested();
            resetLexemeStart();
            scanToken();
        }

        addToken(Tokens.Eof);

        var result = tokens.ToImmutable();
        return result;

        void scanToken()
        {
            var c = advance();
            switch(c)
            {
                case Equal:
                    //check for incremental alternative "=/"
                    var type = match(Alternative) ?
                        TokenType.IncrementalAlternative :
                        TokenType.Equal;
                    addToken(type);
                    break;
                case Alternative:
                    addToken(TokenType.Alternative);
                    break;
                case GroupOpen:
                    addToken(TokenType.GroupOpen);
                    break;
                case GroupClose:
                    addToken(TokenType.GroupClose);
                    break;
                case VariableRepetition:
                    addToken(TokenType.VariableRepetition);
                    break;
                case OptionalSequenceOpen:
                    addToken(TokenType.OptionalSequenceOpen);
                    break;
                case OptionalSequenceClose:
                    addToken(TokenType.OptionalSequenceClose);
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
                        addToken(TokenType.Unknown);
                        _diagnostics.Add(UnexpectedCharacter, getLocation(), getLexeme());
                    }

                    break;
            }

            void addNewLine()
            {
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
            while(isDigit(lookAhead()))
                advancePure();
            addToken(TokenType.SpecificRepetition);
        }

        void advancePure()
        {
            character++;
            current++;
        }

        void resetLexemeStart() => start = current;
        void addToken(TokenOrType tokenOrType)
        {
            var token = tokenOrType.Match(
                token => token,
                type => new Token(type, getLexeme()));
            tokens!.Add(token);
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
            while(lookAhead() == expected)
                advancePure();

            addToken(TokenType.Whitespace);
        }

        void terminal()
        {
            addToken(TokenType.Quote);

            while((lookAhead() != Quote || lookBehind() == Escape) && !isAtEnd())
            {
                if(lookAhead() == NewLine)
                {
                    line++;
                }

                advancePure();
            }

            if(isAtEnd())
            {
                _diagnostics.Add(UnterminatedTerminal, getLocation(), getLexeme());
                return;
            }

            addToken(TokenType.Terminal);

            //consume terminating quote
            advancePure();
            addToken(TokenType.Quote);
        }

        void name()
        {
            while(isAlpha(lookAhead()))
                advancePure();

            addToken(TokenType.Name);
        }
    }
}
