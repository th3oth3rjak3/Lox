namespace Lox.Expressions;

public class Set(Expr obj, Token name, Expr value) : Expr
{
    public Expr Obj { get; } = obj;
    public Token Name { get; } = name;
    public Expr Value { get; } = value;

    public override string ToString()
    {
        return $"{Obj} {Name.Lexeme} = {Value};";
    }

    public override R Accept<R>(IExprVisitor<R> visitor)
    {
        return visitor.VisitSetExpr(this);
    }
}
