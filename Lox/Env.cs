using Lox.Errors;

namespace Lox;

public class Env
{
    private Env? EnclosingEnv { get; set; }
    public Env() { }
    public Env(Env? env)
    {
        EnclosingEnv = env;
    }

    private Dictionary<string, object?> values = [];

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

        if (EnclosingEnv is not null) return EnclosingEnv.Get(name);

        throw new RuntimeError(name, $"Undefined variable '{name.Lexeme}'.");
    }

    public object? GetAt(int distance, string name)
    {
        return Ancestor(distance)?.values[name];
    }

    private Env? Ancestor(int distance)
    {
        var env = this;
        for (int i = 0; i < distance; i++)
        {
            env = env?.EnclosingEnv;
        }

        return env;
    }

    public void Assign(Token name, object? value)
    {

        if (values.ContainsKey(name.Lexeme))
        {
            values[name.Lexeme] = value;
            return;
        }

        if (EnclosingEnv is not null)
        {
            EnclosingEnv.Assign(name, value);
            return;
        }

        throw new RuntimeError(name, $"Undefined variable '{name.Lexeme}'.");
    }

    public void AssignAt(int distance, Token name, object? value)
    {
        var env = Ancestor(distance);
        if (env is null) return;

        env.values[name.Lexeme] = value;
    }
}
