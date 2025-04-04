namespace Lox;

public class Env
{
    private readonly Dictionary<string, object?> values = [];

    public void Define(string name, object? value)
    {
        if (!values.TryAdd(name, value))
        {
            values[name] = value;
        }
    }

    public object? Get(Token name)
    {

        if (values.ContainsKey(name.Lexeme))
        {
            return values[name.Lexeme];
        }

        throw new RuntimeError(name, $"Undefined variable '{name.Lexeme}'.");
    }
}
