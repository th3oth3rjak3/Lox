namespace Lox.Expressions;

public interface ILoxCallable
{
    public object? Call(Interpreter interpreter, List<object?> arguments);
    public int Arity { get; }
}
