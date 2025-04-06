using Lox.Expressions;

namespace Lox.Statements;

public class LoxClass(string name, LoxClass? super, Dictionary<string, LoxFunction> methods) : ILoxCallable
{
    public string Name { get; } = name;
    public LoxClass? Super { get; } = super;
    public Dictionary<string, LoxFunction> Methods { get; } = methods;

    int ILoxCallable.Arity()
    {
        LoxFunction? initializer = FindMethod("init");
        return initializer is null ? 0 : initializer.Arity();
    }

    public LoxFunction? FindMethod(string methodName)
    {
        if (Methods.TryGetValue(methodName, out LoxFunction? method))
        {
            return method;
        }

        if (Super is not null)
        {
            return Super.FindMethod(methodName);
        }

        return null;
    }

    public override string ToString()
    {
        return $"Class: {Name}";
    }

    object? ILoxCallable.Call(Interpreter interpreter, List<object?> arguments)
    {
        LoxInstance instance = new(this);
        LoxFunction? initializer = FindMethod("init");
        if (initializer is not null)
        {
            initializer.Bind(instance).Call(interpreter, arguments);
        }
        return instance;
    }


}
