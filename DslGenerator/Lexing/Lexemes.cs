namespace RhoMicro.CodeAnalysis.DslGenerator.Lexing;

#if DSL_GENERATOR
[IncludeFile]
#endif
static class Lexemes
{
    public const Char Equal = '=';
    public const Char Dash = '-';
    public const Char Period = '.';
    public const Char Alternative = '/';
    public const String IncrementalAlternative = "=/";
    public const Char GroupOpen = '(';
    public const Char GroupClose = ')';
    public const Char VariableRepetition = '*';
    public const Char OptionalSequenceOpen = '[';
    public const Char OptionalSequenceClose = ']';
    public const Char Semicolon = ';';
    public const Char Quote = '"';
    public const Char NewLine = '\n';
    public const Char Space = ' ';
    public const Char Tab = '\t';
    public const Char CarriageReturn = '\r';
    public const Char Escape = '\\';
    public const Char Hash = '#';
    public const String Eof = "";
}
