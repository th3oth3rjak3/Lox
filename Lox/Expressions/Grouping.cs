namespace Lox.Expressions;

public class Grouping(Expr? expression) : Expr
{
    public Expr? Expression { get; set; } = expression;

    public override R Accept<R>(IExprVisitor<R> visitor)
    {
        return visitor.VisitGroupingExpr(this);
    }
}
