using Lox.Statements;

namespace Lox.Expressions;

public class LoxFunction(Function declaration, Env closure, bool isInitializer) : ILoxCallable
{
    public LoxFunction Bind(LoxInstance instance)
    {
        var environment = new Env(closure);
        environment.Define("this", instance);
        return new LoxFunction(declaration, environment, isInitializer);
    }

    public int Arity() => declaration.Parameters.Count;

    public object? Call(Interpreter interpreter, List<object?> arguments)
    {
        var environment = new Env(closure);
        for (var i = 0; i < Arity(); i++)
        {
            environment.Define(declaration.Parameters[i].Lexeme, arguments[i]);
        }
        try
        {
            interpreter.ExecuteBlock(declaration.Body, environment);
        }
        catch (ReturnValue v)
        {
            if (isInitializer)
            {
                return closure.GetAt(0, "this");
            }
            return v.Value;
        }
        if (isInitializer)
        {
            return closure.GetAt(0, "this");
        }
        return null;
    }

    public override string ToString()
    {
        return $"<fun {declaration.Token.Lexeme}>";
    }
}
