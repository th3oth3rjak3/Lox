using System;

namespace Lox.Expressions;

public class Call(Expr callee, Token paren, List<Expr> args) : Expr
{
    public Expr Callee { get; set; } = callee;
    public Token Paren { get; set; } = paren;
    public List<Expr> Arguments { get; set; } = args;

    public override R Accept<R>(IExprVisitor<R> visitor)
    {
        return visitor.VisitCallExpr(this);
    }
}
