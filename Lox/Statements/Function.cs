namespace Lox.Statements;

public class Function(Token token, List<Token> parameters, List<Stmt> body) : Stmt
{
    public Token Token { get; set; } = token;
    public List<Token> Parameters { get; set; } = parameters;
    public List<Stmt> Body { get; set; } = body;

    public override R Accept<R>(IStmtVisitor<R> visitor)
    {
        return visitor.VisitFunctionStmt(this);
    }
}
