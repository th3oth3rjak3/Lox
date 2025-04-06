namespace Lox.Expressions;

public class Variable(Token? token) : Expr
{
    public Token? Token { get; set; } = token;

    public override R Accept<R>(IExprVisitor<R> visitor)
    {
        return visitor.VisitVariableExpr(this);
    }
}
