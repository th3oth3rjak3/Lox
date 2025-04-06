namespace Lox.Expressions;

public class Assign(Token token, Expr value) : Expr
{
    public Token Token { get; set; } = token;
    public Expr Value { get; set; } = value;

    public override R Accept<R>(IExprVisitor<R> visitor)
    {
        return visitor.VisitAssignExpr(this);
    }
}
