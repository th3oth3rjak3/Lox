using Lox.Shared;

namespace Lox;

public class Scanner
{
    private readonly string source;
    private readonly List<Token> tokens = [];
    private int start = 0;
    private int current = 0;
    private readonly int line = 1;

    private bool IsAtEnd => current >= source.Length;

    public Scanner(string sourceCode)
    {
        source = sourceCode;
    }

    public List<Token> ScanTokens()
    {
        while (!IsAtEnd)
        {
            start = current;
            ScanToken();
        }

        tokens.Add(new Token(TokenType.Eof, "", null, line));

        return tokens;
    }

    private void ScanToken()
    {
        var c = Advance();
        switch (c)
        {
            case '(':
                AddToken(TokenType.LeftParen);
                break;
            case ')':
                AddToken(TokenType.RightParen);
                break;
            case '{':
                AddToken(TokenType.LeftBrace);
                break;
            case '}':
                AddToken(TokenType.RightBrace);
                break;
            case ',':
                AddToken(TokenType.Comma);
                break;
            case '.':
                AddToken(TokenType.Dot);
                break;
            case '-':
                AddToken(TokenType.Minus);
                break;
            case '+':
                AddToken(TokenType.Plus);
                break;
            case ';':
                AddToken(TokenType.Semicolon);
                break;
            case '*':
                AddToken(TokenType.Star);
                break;
            case '!':
                AddToken(MatchNext('=') ? TokenType.BangEqual : TokenType.Bang);
                break;
            case '=':
                AddToken(MatchNext('=') ? TokenType.EqualEqual : TokenType.Equal);
                break;
            case '<':
                AddToken(MatchNext('=') ? TokenType.LessEqual : TokenType.Less);
                break;
            case '>':
                AddToken(MatchNext('=') ? TokenType.GreaterEqual : TokenType.Greater);
                break;
            default:
                Program.Error(line, "Unexpected character.");
                break;
        }
    }

    private char Advance()
    {
        var ch = source[current];
        current++;
        return ch;
    }

    private void AddToken(TokenType tokenType)
    {
        AddToken(tokenType, null);
    }

    private void AddToken(TokenType tokenType, Object? literal)
    {
        var text = source[start..current];
        tokens.Add(new Token(tokenType, text, literal, line));
    }

    private bool MatchNext(char expected)
    {
        if (IsAtEnd) return false;
        if (source[current] != expected) return false;
        current++;
        return true;
    }
}
