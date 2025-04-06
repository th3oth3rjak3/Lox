using System;

namespace Lox.Statements;

public class Block(List<Stmt> statements) : Stmt
{
    public List<Stmt> Statements { get; set; } = statements;

    public override R Accept<R>(IStmtVisitor<R> visitor)
    {
        return visitor.VisitBlockStmt(this);
    }
}
