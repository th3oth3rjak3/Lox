namespace Lox.Expressions;

public class Get(Expr obj, Token name) : Expr
{
    public Expr Object { get; } = obj;
    public Token Name { get; } = name;

    public override R Accept<R>(IExprVisitor<R> visitor)
    {
        return visitor.VisitGetExpr(this);
    }
}
