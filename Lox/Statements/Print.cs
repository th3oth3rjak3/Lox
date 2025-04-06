using Lox.Expressions;

namespace Lox.Statements;

public class Print(Expr? expression) : Stmt
{
    public Expr? Expression { get; set; } = expression;

    public override R Accept<R>(IStmtVisitor<R> visitor)
    {
        return visitor.VisitPrintStmt(this);
    }
}
