namespace Lox.Expressions;

public class Super(Token keyword, Token method) : Expr
{
    public Token Keyword { get; } = keyword;
    public Token Method { get; } = method;

    public override T Accept<T>(IExprVisitor<T> visitor)
    {
        return visitor.VisitSuperExpr(this);
    }
}
