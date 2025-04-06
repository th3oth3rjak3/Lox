using Lox.Expressions;
using Lox.Statements;

namespace Lox;

public interface IExprVisitor<T>
{
    T VisitBinaryExpr(Binary expression);
    T VisitUnaryExpr(Unary expression);
    T VisitLiteralExpr(Literal expression);
    T VisitGroupingExpr(Grouping expression);
    T VisitVariableExpr(Variable expression);
    T VisitAssignExpr(Assign expression);
}

public interface IStmtVisitor<T>
{
    T VisitPrintStmt(Print statement);
    T VisitExpressionStmt(Expression statement);
    T VisitVarStmt(Var statement);
    T VisitBlockStmt(Block statement);
}