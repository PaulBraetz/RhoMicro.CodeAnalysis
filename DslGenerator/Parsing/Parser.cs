namespace RhoMicro.CodeAnalysis.DslGenerator.Parsing;

using RhoMicro.CodeAnalysis.DslGenerator.Analysis;
using RhoMicro.CodeAnalysis.DslGenerator.Grammar;
using RhoMicro.CodeAnalysis.DslGenerator.Lexing;
using static RhoMicro.CodeAnalysis.DslGenerator.Analysis.DiagnosticDescriptors;

using System;
using static RhoMicro.CodeAnalysis.DslGenerator.Grammar.Rule;
using System.Text;

/*
RhoMicroBackusNaurForm;

RuleList = [Name ";"] *RuleDefinition;
RuleDefinition = Name "=" Rule ";";

Rule = Binary / Unary / Primary;

Binary = Range / Concatenation / Alternative;
Concatenation = Unary / (Rule Whitespace Rule);
Alternative = Unary / (Rule "/" Rule);
Range = Unary / (SingleAlpha "-" SingleAlpha);

Unary = VariableRepetition / SpecificRepetition;
VariableRepetition = Primary / ("*" Rule);
SpecificRepetition = Primary / (Digit Rule);

Primary = Grouping / OptionalGrouping / TerminalOrName;
Grouping = TerminalOrName / ("(" Rule ")");
OptionalGrouping = TerminalOrName / ("[" Rule "]");
TerminalOrName = Terminal / Any / Name;
Name = Alpha;
Terminal = "\"" . "\"";
Any = ".";

Trivia = Whitespace / NewLine;
Whitespace = Space / Tab *Whitespace;
Space = " ";
Tab = "	";
NewLine = "\n" / "\r\n" / "\r";
SingleAlpha = "a"-"z" / "A"-"Z" / "_";
Alpha = SingleAlpha *SingleAlpha;
Digit = "0"-"9" *Digit;

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

            //Binary = Range / Concatenation / Alternative;
            //Concatenation = Rule Rule;
            //Alternative = Rule "/" Rule;
            var left = parseRange();
            discardTrivia(discardWhitespace: false);
            while(match(TokenType.Slash, TokenType.Whitespace) && !check(TokenType.Semicolon))
            {
                cancellationToken.ThrowIfCancellationRequested();

                var isAlternative = previous().Type == TokenType.Slash ||
                    match(TokenType.Slash); // in case we have [Whitespace: ][Slash:/]

                discardTrivia();
                var right = parseRange();
                left = isAlternative ?
                    new Rule.Alternative(left, right) :
                    new Rule.Concatenation(left, right);
                discardTrivia(discardWhitespace: false);
            }

            return left;
        }

        Rule parseRange()
        {
            cancellationToken.ThrowIfCancellationRequested();

            //Range = Unary / Char "-" Char;
            //Char = "\"" any character, quotes must be escaped "\"";
            if(!check(TokenType.Terminal, t => t.Lexeme.Length == 1))
            {
                return parseUnary();
            }

            var start = advance();
            if(!match(TokenType.Dash))
            {
                return new Terminal(start);
            }

            discardTrivia();
            var end = consume(TokenType.Terminal, "Expected terminal token of length one.", t => t.Lexeme.Length == 1);

            return new Rule.Range(new(start), new(end));
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

            //Primary = Grouping / OptionalGrouping / TerminalOrName;
            //Grouping = TerminalOrName / ("(" Rule ")");
            //OptionalGrouping = TerminalOrName / ("[" Rule "]");
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

            //TerminalOrName = Terminal / Any / Name;
            //Name = Alpha;
            //Terminal = "\"". "\"";
            //Any = ".";
            if(match(TokenType.Name))
            {
                var name = previous();
                return new Reference(new(name));
            }

            if(match(TokenType.Period))
            {
                return Any.Instance;
            }

            discardTrivia();
            return parseTerminal();
        }

        Rule parseTerminal()
        {
            var terminalValue = consume(TokenType.Terminal, "Expected terminal.");
            return new Terminal(terminalValue);
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

        Token consume(TokenType type, String message, Func<Token, Boolean>? matchPredicate = null)
        {
            if(check(type, matchPredicate))
                return advance();

            throw error(peek(), message);
        }

        void discardTrivia(Boolean discardWhitespace = true)
        {
            while(match(TokenType.NewLine, TokenType.Comment) || discardWhitespace && match(TokenType.Whitespace))
            { }
        }

        Boolean check(TokenType type, Func<Token, Boolean>? matchPredicate = null)
        {
            if(isAtEnd() || matchPredicate != null && !matchPredicate.Invoke(peek()))
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

        Token peek() => tokens![current];

        Token previous() => tokens![current - 1];

        ParseException error(Token token, String message)
        {
            diagnostics!.Add(UnexpectedToken, token.Location, token.Type, message);
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
