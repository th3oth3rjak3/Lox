using System.Diagnostics;
using System.Globalization;
using Lox.Errors;
using Lox.Expressions;
using Lox.Statements;

namespace Lox;

public class Interpreter : IExprVisitor<object?>, IStmtVisitor<Unit?>
{
    public Env Globals { get; set; }
    private Env environment;
    private Dictionary<Expr, int> locals = [];

    public Interpreter()
    {
        Globals = new();
        environment = Globals;

        // Define a clock builtin function that returns the current unix time in seconds.
        Globals.Define("clock", new BuiltinFunction(0, (interpreter, args) =>
        {
            return DateTimeOffset.Now.ToUnixTimeMilliseconds() / 1000.0;
        }));
    }

    public void Resolve(Expr expression, int depth)
    {
        locals.Add(expression, depth);
    }

    public void Interpret(List<Stmt> statements)
    {
        try
        {
            foreach (var stmt in statements)
            {
                Execute(stmt);
            }
        }
        catch (RuntimeError error)
        {
            Lox.RuntimeError(error);
        }
    }

    public object? VisitBinaryExpr(Binary expression)
    {
        var left = Evaluate(expression.Left);
        var right = Evaluate(expression.Right);

        switch (expression.Token?.Type)
        {
            case TokenType.Minus:
                CheckNumberOperands(expression.Token, left, right);
                Debug.Assert(left is not null && right is not null);
                return (double)left - (double)right;
            case TokenType.Slash:
                CheckNumberOperands(expression.Token, left, right);
                Debug.Assert(left is not null && right is not null);
                return (double)left / (double)right;
            case TokenType.Star:
                CheckNumberOperands(expression.Token, left, right);
                Debug.Assert(left is not null && right is not null);
                return (double)left * (double)right;
            case TokenType.Plus:
                switch (left)
                {
                    case double ld when right is double rd:
                        return ld + rd;
                    case string ls when right is string rs:
                        return ls + rs;
                }
                break;
            case TokenType.Greater:
                CheckNumberOperands(expression.Token, left, right);
                Debug.Assert(left is not null && right is not null);
                return (double)left > (double)right;
            case TokenType.GreaterEqual:
                CheckNumberOperands(expression.Token, left, right);
                Debug.Assert(left is not null && right is not null);
                return (double)left >= (double)right;
            case TokenType.Less:
                CheckNumberOperands(expression.Token, left, right);
                Debug.Assert(left is not null && right is not null);
                return (double)left < (double)right;
            case TokenType.LessEqual:
                CheckNumberOperands(expression.Token, left, right);
                Debug.Assert(left is not null && right is not null);
                return (double)left <= (double)right;
            case TokenType.BangEqual:
                return !IsEqual(left, right);
            case TokenType.EqualEqual:
                return IsEqual(left, right);
        }
        ;

        return null;
    }
    public object? VisitGroupingExpr(Grouping expression) =>
        Evaluate(expression.Expression);

    public object? VisitLiteralExpr(Literal expression) =>
        expression.Value;
    public object? VisitUnaryExpr(Unary expression)
    {
        var right = Evaluate(expression);

        switch (expression.Token?.Type)
        {
            case TokenType.Minus:
                CheckNumberOperand(expression.Token, right);
                Debug.Assert(right is not null);
                return -(double)right;
            case TokenType.Bang:
                return !IsTruthy(right);
            default:
                return null;
        }
        ;
    }

    private void Execute(Stmt statement)
    {
        statement.Accept(this);
    }

    private object? Evaluate(Expr? expression) => expression?.Accept(this);
    private static bool IsTruthy(object? value)
    {
        return value switch
        {
            null => false,
            bool boolValue => boolValue,
            _ => true,
        };
    }

    private static bool IsEqual(object? left, object? right)
    {
        return left switch
        {
            null when right is null => true,
            null => false,
            _ => left.Equals(right),
        };
    }

    private static void CheckNumberOperand(Token token, object? operand)
    {
        if (operand is double) return;
        throw new RuntimeError(token, "Operand must be a number.");
    }

    private static void CheckNumberOperands(Token token, object? left, object? right)
    {
        if (left is double && right is double) return;
        throw new RuntimeError(token, "Operands must be numbers.");
    }

    private static string Stringify(object? value)
    {
        switch (value)
        {
            case null:
                return "nil";
            case double d:
                {
                    var text = d.ToString(CultureInfo.InvariantCulture);
                    if (text.EndsWith(".0"))
                    {
                        text = text[..^2];
                    }
                    return text;
                }
            case bool b:
                return b.ToString().ToLower();
            default:
                return value.ToString() ?? "nil";
        }
    }
    public Unit? VisitExpressionStmt(Expression stmt)
    {
        Evaluate(stmt.Expr);
        return default;
    }

    public Unit? VisitPrintStmt(Print stmt)
    {
        var value = Evaluate(stmt.Expression);
        Console.WriteLine(Stringify(value));
        return null;
    }

    public Unit? VisitVarStmt(Var stmt)
    {
        object? value = null;
        if (stmt.Initializer != null)
        {
            value = Evaluate(stmt.Initializer);
        }

        var token = stmt.Token;
        if (token is null) throw new ParseError();

        environment.Define(token.Lexeme, value);
        return null;
    }

    public object? VisitVariableExpr(Variable expr)
    {
        return LookupVariable(expr.Token, expr);
    }

    private object? LookupVariable(Token name, Expr expr)
    {
        if (locals.TryGetValue(expr, out var distance))
        {
            return environment.GetAt(distance, name.Lexeme);
        }
        else
        {
            return Globals.Get(name);
        }
    }

    public object? VisitAssignExpr(Assign expr)
    {
        var value = Evaluate(expr.Value);
        if (locals.TryGetValue(expr, out var distance))
        {
            environment.AssignAt(distance, expr.Token, value);
        }
        else
        {
            Globals.Assign(expr.Token, value);
        }

        return value;
    }

    public Unit? VisitBlockStmt(Block stmt)
    {
        ExecuteBlock(stmt.Statements, new Env(environment));
        return null;
    }

    public void ExecuteBlock(List<Stmt> statements, Env env)
    {
        var previous = environment;
        try
        {
            environment = env;
            foreach (var stmt in statements)
            {
                Execute(stmt);
            }
        }
        finally
        {
            environment = previous;
        }
    }

    public Unit? VisitIfStmt(If statement)
    {
        if (IsTruthy(Evaluate(statement.Condition)))
        {
            Execute(statement.ThenBranch);
        }
        else if (statement.ElseBranch is not null)
        {
            Execute(statement.ElseBranch);
        }

        return null;
    }

    public object? VisitLogicalExpr(Logical expression)
    {
        var left = Evaluate(expression.Left);

        if (expression.Token.Type == TokenType.Or)
        {
            if (IsTruthy(left)) return left;
        }
        else
        {
            if (!IsTruthy(left))
            {
                return left;
            }
        }

        return Evaluate(expression.Right);
    }

    public Unit? VisitWhileStmt(While statement)
    {
        while (IsTruthy(Evaluate(statement.Condition)))
        {
            Execute(statement.Body);
        }

        return null;
    }

    public object? VisitCallExpr(Call expression)
    {
        var callee = Evaluate(expression.Callee);
        List<object?> arguments = [];

        foreach (var arg in expression.Arguments)
        {
            arguments.Add(Evaluate(arg));
        }

        if (callee is null || callee is not ILoxCallable)
        {
            throw new RuntimeError(expression.Paren, "Can only call functions and classes.");
        }

        ILoxCallable function = (ILoxCallable)callee;

        if (arguments.Count != function.Arity())
        {
            throw new RuntimeError(expression.Paren, $"Expected {function.Arity()} arguments, but got {arguments.Count}.");
        }
        return function.Call(this, arguments);
    }

    public Unit? VisitFunctionStmt(Function statement)
    {
        LoxFunction fn = new(statement, environment, false);
        environment.Define(statement.Token.Lexeme, fn);
        return null;
    }

    public Unit? VisitReturnStmt(Return statement)
    {
        object? value = null;
        if (statement.Value is not null)
        {
            value = Evaluate(statement.Value);
        }

        throw new ReturnValue(value);
    }

    public Unit? VisitClassStmt(Class statement)
    {
        object? superclass = null;
        if (statement.Super is not null)
        {
            superclass = Evaluate(statement.Super);
            if (!(superclass is LoxClass))
            {
                throw new RuntimeError(statement.Super.Token, "Superclass must be a class.");
            }
        }

        environment.Define(statement.Name.Lexeme, null);

        if (statement.Super is not null)
        {
            environment = new Env(environment);
            environment.Define("super", superclass);
        }

        var methods = new Dictionary<string, LoxFunction>();
        foreach (Function fun in statement.Methods)
        {
            LoxFunction method = new(fun, environment, fun.Token.Lexeme == "init");
            methods[fun.Token.Lexeme] = method;
        }

        LoxClass klass = new(statement.Name.Lexeme, (LoxClass?)superclass, methods);

        if (superclass is not null && environment.EnclosingEnv is not null)
        {
            environment = environment.EnclosingEnv;
        }

        environment.Assign(statement.Name, klass);
        return null;
    }

    public object? VisitGetExpr(Get expression)
    {
        object? obj = Evaluate(expression.Object);
        if (obj is LoxInstance instance)
        {
            return instance.Get(expression.Name);
        }

        throw new RuntimeError(expression.Name, "Only instances have properties.");
    }

    public object? VisitSetExpr(Set expression)
    {
        object? obj = Evaluate(expression.Obj);
        if (obj is LoxInstance instance)
        {
            object? value = Evaluate(expression.Value);
            instance.Set(expression.Name, value);
            return value;
        }

        throw new RuntimeError(expression.Name, "Only instances have fields.");

    }

    public object? VisitThisExpr(This expression)
    {
        return LookupVariable(expression.Keyword, expression);
    }

    public object? VisitSuperExpr(Super expression)
    {
        int distance = locals[expression];
        LoxClass? superclass = (LoxClass?)environment.GetAt(distance, "super");
        LoxInstance? obj = (LoxInstance?)environment.GetAt(distance - 1, "this");
        LoxFunction? method = null;

        if (superclass is not null)
        {
            method = superclass.FindMethod(expression.Method.Lexeme);
        }

        if (method is null || obj is null)
        {
            throw new RuntimeError(expression.Method, "Undefined property '" + expression.Method.Lexeme + "'.");
        }

        return method.Bind(obj);
    }
}
