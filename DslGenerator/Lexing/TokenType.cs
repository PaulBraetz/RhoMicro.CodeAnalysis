namespace RhoMicro.CodeAnalysis.DslGenerator.Lexing;

#if DSL_GENERATOR
[IncludeFile]
#endif
enum TokenType
{
    Unknown,
    Name,
    Equal,
    Whitespace,
    Slash,
    SlashEqual,
    ParenLeft,
    ParenRight,
    Star,
    Number,
    BracketLeft,
    BracketRight,
    Semicolon,
    Comment,
    NewLine,
    Terminal,
    Dash,
    Eof
}
