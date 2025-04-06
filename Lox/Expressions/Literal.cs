namespace Lox.Expressions;

public class Literal(object? value) : Expr
{
    public object? Value { get; set; } = value;

    public override R Accept<R>(IExprVisitor<R> visitor)
    {
        return visitor.VisitLiteralExpr(this);
    }
}
