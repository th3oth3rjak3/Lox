using Lox.Shared;

namespace Lox;

public class Scanner
{
    private readonly string source;
    private readonly List<Token> tokens = [];
    private int start = 0;
    private int current = 0;
    private int line = 1;

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
            case '/':
                if (MatchNext('/'))
                {
                    // A Comment will go to the end of the line until it hits a newline character.
                    while (Peek() != '\n') Advance();
                }
                else
                {
                    AddToken(TokenType.Slash);
                }
                break;
            case ' ':
            case '\r':
            case '\t':
                // Ignore Whitespace
                break;
            case '\n':
                line++;
                break;
            case '"':
                HandleString();
                break;
            default:
                if (IsDigit(c))
                {
                    HandleNumber();
                }
                else if (IsAlpha(c))
                {
                    HandleIdentifier();
                }
                else
                {
                    Program.Error(line, "Unexpected character.");
                }
                break;
        }
    }

    private char Advance()
    {
        var ch = source[current];
        current++;
        return ch;
    }

    private char Peek()
    {
        if (IsAtEnd) return '\0';
        return source[current];
    }

    private char PeekNext()
    {
        if (current + 1 >= source.Length) return '\0';
        return source[current + 1];
    }

    private void AddToken(TokenType tokenType) =>
        AddToken(tokenType, null);

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

    private void HandleString()
    {
        while (Peek() != '"' && !IsAtEnd)
        {
            if (Peek() == '\n') line++;
            Advance();
        }

        if (IsAtEnd)
        {
            Program.Error(line, "Unterminated string.");
            return;
        }

        // Move beyond the closing quote
        Advance();

        var value = source[(start + 1)..(current - 1)];
        AddToken(TokenType.String, value);
    }

    private void HandleNumber()
    {
        while (IsDigit(Peek())) Advance();

        // Look for a decimal
        if (Peek() == '.' && IsDigit(PeekNext()))
        {
            Advance();
            while (IsDigit(Peek())) Advance();
        }

        AddToken(TokenType.Number, Double.Parse(source[start..current]));
    }

    private void HandleIdentifier()
    {
        while (IsAlphaNumeric(Peek())) Advance();
        var text = source[start..current];
        // If we can't find a reserved word, it must be a user generated identifier.
        var type = ReservedWords.Get(text) ?? TokenType.Identifier;
        AddToken(type);
    }

    private static bool IsDigit(char ch) => ch >= '0' && ch <= '9';
    private static bool IsAlpha(char ch) =>
        (ch >= 'a' && ch <= 'z')
        || (ch >= 'A' && ch <= 'Z')
        || (ch == '_');

    private static bool IsAlphaNumeric(char ch) =>
        IsAlpha(ch) || IsDigit(ch);
}
