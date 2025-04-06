namespace Lox.Expressions;

public class This(Token keyword) : Expr
{
    public Token Keyword { get; } = keyword;

    public override T Accept<T>(IExprVisitor<T> visitor)
    {
        return visitor.VisitThisExpr(this);
    }
}
