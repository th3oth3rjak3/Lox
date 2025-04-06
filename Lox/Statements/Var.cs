using Lox.Expressions;

namespace Lox.Statements;

public class Var(Token token, Expr? initializer) : Stmt
{
    public Token Token { get; set; } = token;
    public Expr? Initializer { get; set; } = initializer;

    public override R Accept<R>(IStmtVisitor<R> visitor)
    {
        return visitor.VisitVarStmt(this);
    }
}
