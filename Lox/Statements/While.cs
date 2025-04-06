using System;
using Lox.Expressions;

namespace Lox.Statements;

public class While(Expr condition, Stmt body) : Stmt
{
    public Expr Condition { get; set; } = condition;
    public Stmt Body { get; set; } = body;

    public override R Accept<R>(IStmtVisitor<R> visitor)
    {
        return visitor.VisitWhileStmt(this);
    }
}
