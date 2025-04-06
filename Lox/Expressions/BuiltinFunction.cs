namespace Lox.Expressions;

public class BuiltinFunction(int arity, Func<Interpreter, List<object?>, object?> func) : ILoxCallable
{
    public int Arity() => arity;

    public object? Call(Interpreter interpreter, List<object?> arguments)
    {
        return func(interpreter, arguments);
    }

    public override string ToString()
    {
        return "<built-in fun>";
    }
}
