#pragma warning disable IDE2003 // Blank line required between block and subsequent statement
namespace RhoMicro.CodeAnalysis.DslGenerator.Parsing;

using RhoMicro.CodeAnalysis.DslGenerator.Analysis;
using RhoMicro.CodeAnalysis.DslGenerator.Grammar;
using RhoMicro.CodeAnalysis.DslGenerator.Lexing;

using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

using static RhoMicro.CodeAnalysis.DslGenerator.Analysis.DiagnosticDescriptors;

/*
Precedence Table (low to high):
Names                                   Associativity
Concatenation, Alternative              Left
VariableRepetition, SpecificRepetition  Right
SequenceGroup, OptionalSequence         Left

RuleList = *RuleDefinition;
RueDefinition = RuleName "=" Rule;
Rule = Binary / Unary / Primary;

Binary = Unary / Concatenation / Alternative;
Concatenation = Rule Rule;
Alternative = Rule "/" Rule;

Unary = Primary / VariableRepetition / SpecificRepetition;
VariableRepetition = "*" Rule;
SpecificRepetition = NUMBER Rule;

Primary = OptionalSequence / Sequence / Terminal;
OptionalSequence = "[" Rule "]";
Sequence = "(" Rule ")";
Terminal = "\"" any string, quotes must be escaped "\"";
*/

static class Parser
{
    public static ParseResult Parse(ImmutableArray<Token> tokens, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
        //cancellationToken.ThrowIfCancellationRequested();
        //var current = 0;
        //var diagnostics = new DiagnosticsCollection();

        //var result = new ParseResult(ruleList, diagnostics);

        //return result;
    }
}
sealed class UnexpectedTokenException(Token token, TokenType expectedTokenType) : ParseException
{
    public Token Token { get; } = token;
    public TokenType ExpectedTokenType { get; } = expectedTokenType;
}
class ParseException : Exception
{
    public ParseException() { }
    public ParseException(String message) : base(message) { }
}
readonly record struct ParseResult(RuleList RuleList, DiagnosticsCollection Diagnostics);
