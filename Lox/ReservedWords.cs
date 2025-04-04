namespace Lox;

/// <summary>
/// A static class used to manage reserved words.
/// </summary>
public static class ReservedWords
{
    /// <summary>
    /// A static dictionary of Lox reserved words and their token types.
    /// </summary>
    private readonly static Dictionary<string, TokenType> LoxReservedWords =
        new()
        {
            { "and", TokenType.And },
            { "class", TokenType.Class },
            { "else", TokenType.Else },
            { "false", TokenType.False },
            { "for", TokenType.For },
            { "fun", TokenType.Fun },
            { "if", TokenType.If },
            { "nil", TokenType.Nil },
            { "or", TokenType.Or },
            { "print", TokenType.Print },
            { "return", TokenType.Return },
            { "super", TokenType.Super },
            { "this", TokenType.This },
            { "true", TokenType.True },
            { "var", TokenType.Var },
            { "while", TokenType.While },
        };

    /// <summary>
    /// Try to get a reserved word token type by name.
    /// </summary>
    /// <param name="name">The name of the reserved word to check.</param>
    /// <returns>The token type when found, otherwise null.</returns>
    public static TokenType? Get(string name)
    {
        if (LoxReservedWords.TryGetValue(name, out var reservedWord))
            return reservedWord;

        return null;
    }
}