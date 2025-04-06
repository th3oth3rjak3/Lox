using Lox.Expressions;

namespace Lox.Statements;

public class Return(Token keyword, Expr? value) : Stmt
{
    public Token Keyword { get; set; } = keyword;
    public Expr? Value { get; set; } = value;

    public override R Accept<R>(IStmtVisitor<R> visitor)
    {
        return visitor.VisitReturnStmt(this);
    }
}
