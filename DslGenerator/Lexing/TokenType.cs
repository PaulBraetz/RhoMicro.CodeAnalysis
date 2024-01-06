namespace RhoMicro.CodeAnalysis.DslGenerator.Lexing;

enum TokenType
{
    Unknown,
    Name,
    Equal,
    Whitespace,
    Alternative,
    IncrementalAlternative,
    GroupOpen,
    GroupClose,
    VariableRepetition,
    SpecificRepetition,
    OptionalSequenceOpen,
    OptionalSequenceClose,
    Hash,
    Semicolon,
    Comment,
    NewLine,
    Quote,
    Terminal,
    Eof
}
