using Lox.Statements;

namespace Lox.Expressions;

public class LoxFunction(Function declaration, Env closure) : ILoxCallable
{

    public int Arity => declaration.Parameters.Count;

    public object? Call(Interpreter interpreter, List<object?> arguments)
    {
        var environment = new Env(closure);
        for (var i = 0; i < Arity; i++)
        {
            environment.Define(declaration.Parameters[i].Lexeme, arguments[i]);
        }
        try
        {
            interpreter.ExecuteBlock(declaration.Body, environment);
        }
        catch (ReturnValue v)
        {
            return v.Value;
        }

        return null;
    }

    public override string ToString()
    {
        return $"<fun {declaration.Token.Lexeme}>";
    }
}
