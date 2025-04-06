namespace Lox.Expressions;

public class Unary(Token token, Expr right) : Expr
{
    public Token Token { get; set; } = token;
    public Expr Right { get; set; } = right;

    public override R Accept<R>(IExprVisitor<R> visitor)
    {
        return visitor.VisitUnaryExpr(this);
    }
}
