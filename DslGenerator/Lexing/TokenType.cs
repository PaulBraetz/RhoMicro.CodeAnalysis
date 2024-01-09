namespace RhoMicro.CodeAnalysis.DslGenerator.Lexing;
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
    Hash,
    Semicolon,
    Comment,
    NewLine,
    Quote,
    Terminal,
    Eof
}
