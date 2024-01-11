#if DSL_GENERATOR
namespace RhoMicro.CodeAnalysis.DslGenerator.Parsing;
#pragma warning disable CA1822 // Mark members as static
#else
#pragma warning disable
#nullable enable
namespace RhoMicro.CodeAnalysis.DslGenerator.Generated.Parsing;
#endif

#if DSL_GENERATOR
using RhoMicro.CodeAnalysis.DslGenerator.Analysis;
using RhoMicro.CodeAnalysis.DslGenerator.Grammar;
using RhoMicro.CodeAnalysis.DslGenerator.Lexing;
using static RhoMicro.CodeAnalysis.DslGenerator.Analysis.DiagnosticDescriptors;
#else
using RhoMicro.CodeAnalysis.DslGenerator.Generated.Analysis;
using RhoMicro.CodeAnalysis.DslGenerator.Generated.Grammar;
using RhoMicro.CodeAnalysis.DslGenerator.Generated.Lexing;
using static RhoMicro.CodeAnalysis.DslGenerator.Generated.Analysis.DiagnosticDescriptors;
#endif

using System;

/*
Precedence Table (low to high):
Names                                   Associativity
Concatenation, Alternative              Left
VariableRepetition, SpecificRepetition  Right
SequenceGroup, OptionalSequence         Left

RuleList = [Name ";"] *RuleDefinition;
RueDefinition = Name "=" Rule;
Name = ALPHA
Rule = Binary / Unary / Primary;

Binary = Unary / Concatenation / Alternative;
Concatenation = Rule Rule;
Alternative = Rule "/" Rule;

Unary = Primary / VariableRepetition / SpecificRepetition;
VariableRepetition = "*" Rule;
SpecificRepetition = NUMBER Rule;

Primary = TerminalOrName / OptionalSequence / Sequence;
OptionalSequence = "[" Rule "]";
Sequence = "(" Rule ")";

TerminalOrName = Terminal / Name;
Terminal = "\"" any string, quotes must be escaped "\"";
*/

#if DSL_GENERATOR
[IncludeFile]
#endif
sealed class Parser
{
    sealed class ParseException : Exception { }
    public static Parser Instance { get; } = new();
    public ParseResult Parse(SourceText sourceText, CancellationToken cancellationToken
#if DSL_GENERATOR
    , String filePath = ""
#endif
        ) =>
        Parse(Tokenizer.Instance.Tokenize(sourceText, cancellationToken
#if DSL_GENERATOR
            , filePath
#endif
            ), cancellationToken);
    public ParseResult Parse(TokenizeResult tokenizeResult, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var current = 0;
        var (tokens, tokenizeDiagnostics) = tokenizeResult;
        var diagnostics = new DiagnosticsCollection
        {
            tokenizeDiagnostics
        };

        var ruleList = parseRuleList();

        return new(ruleList, diagnostics);

        RuleList parseRuleList()
        {
            cancellationToken.ThrowIfCancellationRequested();

            //RuleList = Name ";" *RuleDefinition;
            Name? name = null;
            try
            {
                discardTrivia();
                if(match(TokenType.Name))
                {
                    var nameToken = previous();
                    discardTrivia();
                    if(match(TokenType.Semicolon))
                    {
                        name = new(nameToken);
                    } else
                    {
                        current = 0;
                    }
                }
            } catch(ParseException)
            {
                synchronize();
            }

            var definitions = new List<RuleDefinition>();
            while(!isAtEnd())
            {
                cancellationToken.ThrowIfCancellationRequested();

                if(tryParseDefinition(out var def))
                {
                    definitions.Add(def!);
                }
            }

            var result = name != null ?
                new NamedRuleList(name, definitions) :
                new RuleList(definitions);

            return result;
        }

        Boolean tryParseDefinition(out RuleDefinition? definition)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                discardTrivia();
                definition = parseDefinition();
                discardTrivia();
                return true;
            } catch(ParseException)
            {
                synchronize();
                definition = default;
                return false;
            }
        }

        RuleDefinition parseDefinition()
        {
            cancellationToken.ThrowIfCancellationRequested();

            //RueDefinition = Name "=" Rule;
            var name = consume(TokenType.Name, "Expected rule name.").Lexeme.ToString() ?? String.Empty;
            discardTrivia();
            var isInremental = match(TokenType.SlashEqual);
            if(!isInremental)
            {
                _ = consume(TokenType.Equal, "Expected '=' or '/=' after rule name.");
            }

            discardTrivia();
            var rule = parseRule();
            discardTrivia();
            _ = consume(TokenType.Semicolon, "Expected ';' after rule definition.");
            RuleDefinition result = isInremental ?
                new RuleDefinition.Incremental(new(name), rule) :
                new RuleDefinition.New(new(name), rule);

            return result;
        }

        Rule parseRule()
        {
            cancellationToken.ThrowIfCancellationRequested();

            //Rule = Binary / Unary / Primary;
            return parseBinary();
        }

        Rule parseBinary()
        {
            cancellationToken.ThrowIfCancellationRequested();

            //Binary = Unary / Concatenation / Alternative;
            //Concatenation = Rule Rule;
            //Alternative = Rule "/" Rule;
            var left = parseUnary();
            discardTrivia(discardWhitespace: false);
            while(match(TokenType.Slash, TokenType.Whitespace))
            {
                cancellationToken.ThrowIfCancellationRequested();

                var isAlternative = previous().Type == TokenType.Slash ||
                    match(TokenType.Slash); // in case we have [Whitespace: ][Slash:/]

                discardTrivia();

                var right = parseUnary();
                left = isAlternative ?
                    new Rule.Alternative(left, right) :
                    new Rule.Concatenation(left, right);
            }

            return left;
        }

        Rule parseUnary()
        {
            cancellationToken.ThrowIfCancellationRequested();

            //Unary = Primary / VariableRepetition / SpecificRepetition;
            //VariableRepetition = "*" Rule;
            //SpecificRepetition = NUMBER Rule;
            if(match(TokenType.Star, TokenType.Number))
            {
                var @operator = previous();
                var isVariable = @operator.Type == TokenType.Star;
                discardTrivia();
                var operand = parsePrimary();
                return isVariable ?
                    new Rule.VariableRepetition(operand) :
                    new Rule.SpecificRepetition(Int32.Parse(@operator.Lexeme.ToString()), operand);
            }

            discardTrivia();
            return parsePrimary();
        }

        Rule parsePrimary()
        {
            cancellationToken.ThrowIfCancellationRequested();

            //Primary = TerminalOrName / OptionalSequence / Sequence;
            //OptionalSequence = "[" Rule "]";
            //Sequence = "(" Rule ")";
            var isSequence = match(TokenType.ParenLeft); //if false, none will have been consumed
            if(!(isSequence || match(TokenType.BracketLeft))) //short cicuit prevents double consumption otherwise
            {
                discardTrivia();
                return parseTerminalOrName();
            }
            //is either sequence or optional sequence
            discardTrivia();
            var rule = parseRule();
            discardTrivia();
            _ = isSequence ?
                consume(TokenType.ParenRight, "Expected ')' after rule grouping.") :
                consume(TokenType.BracketRight, "Expected ']' after optional rule grouping.");
            Rule result = isSequence ?
                new Rule.Grouping(rule) :
                new Rule.OptionalGrouping(rule);

            return result;
        }

        Rule parseTerminalOrName()
        {
            cancellationToken.ThrowIfCancellationRequested();

            //TerminalOrName = Terminal / Name;
            //Terminal = "\"" any string, quotes must be escaped "\"";
            if(match(TokenType.Name))
            {
                var name = previous();
                return new Rule.Reference(new(name));
            }

            discardTrivia();
            _ = consume(TokenType.Quote, "Expected '\"' before terminal.");
            var terminalValue = String.Empty;
            if(!match(TokenType.Quote))
            {
                terminalValue = consume(TokenType.Terminal, "Expected terminal after quote.")
                    .Lexeme.ToString() ?? String.Empty;
                discardTrivia();
                _ = consume(TokenType.Quote, "Expected '\"' after terminal.");
            }

            return Rule.Terminal.Create(terminalValue);
        }

        Boolean match(params TokenType[] types)
        {
            foreach(var type in types)
            {
                if(check(type))
                {
                    _ = advance();
                    return true;
                }
            }

            return false;
        }

        Token consume(TokenType type, String message)
        {
            if(check(type))
                return advance();

            throw error(peek(), message);
        }

        void discardTrivia(Boolean discardWhitespace = true)
        {
            while(match(TokenType.NewLine, TokenType.Hash, TokenType.Comment) || discardWhitespace && match(TokenType.Whitespace))
            { }
        }

        Boolean check(TokenType type)
        {
            if(isAtEnd())
                return false;
            return peek().Type == type;
        }

        Token advance()
        {
            if(!isAtEnd())
                current++;
            return previous();
        }

        Boolean isAtEnd() => peek().Type == TokenType.Eof;

        Token peek() => tokens[current];

        Token previous() => tokens[current - 1];

        ParseException error(Token token, String message)
        {
            diagnostics.Add(UnexpectedToken, token.Location, token.Type, message);
            return new ParseException();
        }

        void synchronize()
        {
            _ = advance();

            while(!isAtEnd())
            {
                if(previous().Type == TokenType.Semicolon)
                    return;

                _ = advance();
            }
        }
    }
}
