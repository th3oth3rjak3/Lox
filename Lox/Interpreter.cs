using System.Diagnostics;
using System.Globalization;

namespace Lox;

public class Interpreter : IExprVisitor<object?>, IStmtVisitor<Unit>
{
    private readonly Env environment = new();

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
            default:
                return value.ToString() ?? "nil";
        }
    }
    public Unit VisitExpressionStmt(Expression stmt)
    {
        Evaluate(stmt.Expr);
        return default;
    }

    public Unit VisitPrintStmt(Print stmt)
    {
        var value = Evaluate(stmt.Expression);
        Console.WriteLine(Stringify(value));
        return default;
    }

    public Unit VisitVarStmt(Var stmt)
    {
        object? value = null;
        if (stmt.Initializer != null)
        {
            value = Evaluate(stmt.Initializer);
        }

        var token = stmt.Name;
        if (token is null) throw new ParseError();

        environment.Define(token.Lexeme, value);
        return default;
    }

    public object? VisitVariableExpr(Variable expr)
    {
        var token = expr.Name;
        if (token is null) return null;
        return environment.Get(token);
    }
}