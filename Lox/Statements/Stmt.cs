namespace Lox.Statements;

public abstract class Stmt
{
    public abstract R Accept<R>(IStmtVisitor<R> visitor);
}
