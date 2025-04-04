using System.Text;

namespace Lox;

public class AstPrinter : IExprVisitor<string>
{
    public string Print(Expr? expression) => expression?.Accept(this) ?? "nil";
    public string VisitBinaryExpr(Binary expression) =>
        Parenthesize(expression.Token?.Lexeme, expression.Left, expression.Right);

    public string VisitGroupingExpr(Grouping expression) =>
        Parenthesize("group", expression.Expression);

    public string VisitLiteralExpr(Literal expression) =>
        expression.Value?.ToString() == null
            ? "nil"
            : expression.Value.ToString()!;

    public string VisitUnaryExpr(Unary expression) =>
        Parenthesize(expression.Token?.Lexeme, expression.Right);

    public string VisitVariableExpr(Variable expr)
    {
        throw new NotImplementedException();
    }

    private string Parenthesize(string? name, params Expr?[] expressions)
    {
        var builder = new StringBuilder();
        builder.Append('(').Append(name);
        expressions.ToList().ForEach(e =>
        {
            builder.Append(' ');
            builder.Append(e?.Accept(this));
        });
        builder.Append(')');

        return builder.ToString();
    }
}