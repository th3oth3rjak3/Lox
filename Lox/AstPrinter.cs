using System.Text;

namespace Lox;

public class AstPrinter : IVisitor<string>
{
    public string Print(Expression? expression) => expression?.Accept(this) ?? "nil";
    public string VisitBinaryExpression(Binary expression) =>
        Parenthesize(expression.Token.Lexeme, expression.Left, expression.Right);
    
    public string VisitGroupingExpression(Grouping expression) => 
        Parenthesize("group", expression.Expression);

    public string VisitLiteralExpression(Literal expression) =>
        expression.Value?.ToString() == null 
            ? "nil" 
            : expression.Value.ToString()!;
    
    public string VisitUnaryExpression(Unary expression) => 
        Parenthesize(expression.Token.Lexeme, expression.Right);

    private string Parenthesize(string name, params Expression[] expressions)
    {
        var builder = new StringBuilder();
        builder.Append('(').Append(name);
        expressions.ToList().ForEach(e =>
        {
            builder.Append(' ');
            builder.Append(e.Accept(this));
        });
        builder.Append(')');
        
        return builder.ToString();
    }
}