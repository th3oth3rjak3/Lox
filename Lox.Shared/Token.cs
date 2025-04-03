namespace Lox.Shared;

public record Token(TokenType Type, string Lexeme, Object? Literal, int Line)
{
    public override string ToString() =>
        $"{Type} {Lexeme} {Literal}";
}
