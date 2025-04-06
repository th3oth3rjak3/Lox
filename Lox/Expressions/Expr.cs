namespace Lox.Expressions;

public abstract class Expr
{
    public abstract R Accept<R>(IExprVisitor<R> visitor);
}