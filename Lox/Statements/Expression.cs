using Lox.Expressions;

namespace Lox.Statements;

public class Expression(Expr? expression) : Stmt
{
    public Expr? Expr { get; set; } = expression;

    public override R Accept<R>(IStmtVisitor<R> visitor)
    {
        return visitor.VisitExpressionStmt(this);
    }
}
