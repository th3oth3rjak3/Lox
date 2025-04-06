using Lox.Expressions;

namespace Lox.Statements;

public class If(Expr condition, Stmt thenBranch, Stmt? elseBranch) : Stmt
{
    public Expr Condition { get; set; } = condition;
    public Stmt ThenBranch { get; set; } = thenBranch;
    public Stmt? ElseBranch { get; set; } = elseBranch;

    public override R Accept<R>(IStmtVisitor<R> visitor)
    {
        return visitor.VisitIfStmt(this);
    }
}
