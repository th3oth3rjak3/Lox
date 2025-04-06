using Lox.Expressions;

namespace Lox.Statements;

public class Class(Token name, Variable? super, List<Function> methods) : Stmt
{
    public Token Name { get; set; } = name;
    public Variable? Super { get; set; } = super;
    public List<Function> Methods { get; set; } = methods;

    public override R Accept<R>(IStmtVisitor<R> visitor)
    {
        return visitor.VisitClassStmt(this);
    }
}
