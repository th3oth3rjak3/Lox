using System;

namespace Lox.Expressions;

public class Logical(Expr left, Token token, Expr right) : Expr
{
    public Expr Left { get; set; } = left;
    public Token Token { get; set; } = token;
    public Expr Right { get; set; } = right;

    public override R Accept<R>(IExprVisitor<R> visitor)
    {
        return visitor.VisitLogicalExpr(this);
    }
}
