using System.Diagnostics;
using System.Globalization;
using System.Xml.Xsl;

namespace Lox;

public class Interpreter : IVisitor<object?>
{
    public void Interpret(Expression? expression)
    {
        try
        {
            var value = Evaluate(expression);
            Console.WriteLine(Stringify(value));
        }
        catch (RuntimeError error)
        {
            Lox.RuntimeError(error);
        }
    }
    
    public object? VisitBinaryExpression(Binary expression)
    {
        var left = Evaluate(expression.Left);
        var right = Evaluate(expression.Right);

        switch (expression.Token.Type)
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
        };

        return null;
    }
    public object? VisitGroupingExpression(Grouping expression) => 
        Evaluate(expression.Expression);
    
    public object? VisitLiteralExpression(Literal expression) => 
        expression.Value;
    public object? VisitUnaryExpression(Unary expression)
    {
        var right = Evaluate(expression);

        switch (expression.Token.Type)
        { 
            case TokenType.Minus: 
                CheckNumberOperand(expression.Token, right);
                return -(double)right!; // Operand is checked in the CheckNumberOperand function.
            case TokenType.Bang:
                return !IsTruthy(right);
            default:
                return null;
        };
    }
    
    private object? Evaluate(Expression? expression) => expression?.Accept(this);
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
    
    private void CheckNumberOperands(Token token, object? left, object? right)
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
}