namespace Lox.Expressions;

public class Binary(Expr left, Token token, Expr right) : Expr
{
    public Expr Left { get; set; } = left;
    public Expr Right { get; set; } = right;
    public Token Token { get; set; } = token;

    public override R Accept<R>(IExprVisitor<R> visitor)
    {
        return visitor.VisitBinaryExpr(this);
    }
}
