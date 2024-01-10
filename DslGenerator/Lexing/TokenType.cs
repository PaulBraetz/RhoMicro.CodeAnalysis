#if DSL_GENERATOR
namespace RhoMicro.CodeAnalysis.DslGenerator.Lexing;
#else
#pragma warning disable
#nullable enable
namespace RhoMicro.CodeAnalysis.DslGenerator.Generated.Lexing;
#endif

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
    Hash,
    Semicolon,
    Comment,
    NewLine,
    Quote,
    Terminal,
    Eof
}
