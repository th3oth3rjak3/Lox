namespace Lox;

public class Scanner(string sourceCode)
{
    private readonly List<Token> _tokens = [];
    private int _start = 0;
    private int _current = 0;
    private int _line = 1;

    private bool IsAtEnd => _current >= sourceCode.Length;

    public List<Token> ScanTokens()
    {
        while (!IsAtEnd)
        {
            _start = _current;
            ScanToken();
        }

        _tokens.Add(new Token(TokenType.Eof, "", null, _line));

        return _tokens;
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
                    while (Peek() != '\n' && !IsAtEnd) Advance();
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
                _line++;
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
                    Lox.Error(_line, "Unexpected character.");
                }
                break;
        }
    }

    private char Advance()
    {
        var ch = sourceCode[_current];
        _current++;
        return ch;
    }

    private char Peek() =>
        IsAtEnd 
            ? '\0' 
            : sourceCode[_current];

    private char PeekNext() =>
        _current + 1 >= sourceCode.Length 
            ? '\0' 
            : sourceCode[_current + 1];

    private void AddToken(TokenType tokenType) =>
        AddToken(tokenType, null);

    private void AddToken(TokenType tokenType, object? literal)
    {
        var text = sourceCode[_start.._current];
        _tokens.Add(new Token(tokenType, text, literal, _line));
    }

    private bool MatchNext(char expected)
    {
        if (IsAtEnd) return false;
        if (sourceCode[_current] != expected) return false;
        _current++;
        return true;
    }

    private void HandleString()
    {
        while (Peek() != '"' && !IsAtEnd)
        {
            if (Peek() == '\n') _line++;
            Advance();
        }

        if (IsAtEnd)
        {
            Lox.Error(_line, "Unterminated string.");
            return;
        }

        // Move beyond the closing quote
        Advance();

        var value = sourceCode[(_start + 1)..(_current - 1)];
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

        AddToken(TokenType.Number, double.Parse(sourceCode[_start.._current]));
    }

    private void HandleIdentifier()
    {
        while (IsAlphaNumeric(Peek())) Advance();
        var text = sourceCode[_start.._current];
        // If we can't find a reserved word, it must be a user generated identifier.
        var type = ReservedWords.Get(text) ?? TokenType.Identifier;
        AddToken(type);
    }

    private static bool IsDigit(char ch) => ch is >= '0' and <= '9';
    private static bool IsAlpha(char ch) =>
        ch is (>= 'a' and <= 'z') or (>= 'A' and <= 'Z') or '_';

    private static bool IsAlphaNumeric(char ch) =>
        IsAlpha(ch) || IsDigit(ch);
}