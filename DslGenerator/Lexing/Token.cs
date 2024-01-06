namespace RhoMicro.CodeAnalysis.DslGenerator.Lexing;

readonly record struct Token(TokenType Type, Lexeme Lexeme)
{
    public override String ToString() => $"[{Type}:{(Type == TokenType.NewLine ? "\\n" : Lexeme.ToString())}]";
}
