using Lox.Expressions;
using Lox.Statements;

namespace Lox;

public interface IExprVisitor<T>
{
    T VisitAssignExpr(Assign expression);
    T VisitBinaryExpr(Binary expression);
    T VisitCallExpr(Call expression);
    T VisitGetExpr(Get expression);
    T VisitGroupingExpr(Grouping expression);
    T VisitLiteralExpr(Literal expression);
    T VisitLogicalExpr(Logical expression);
    T VisitSetExpr(Set expression);
    T VisitSuperExpr(Super expression);
    T VisitThisExpr(This expression);
    T VisitUnaryExpr(Unary expression);
    T VisitVariableExpr(Variable expression);
}

public interface IStmtVisitor<T>
{
    T VisitBlockStmt(Block statement);
    T VisitClassStmt(Class statement);
    T VisitExpressionStmt(Expression statement);
    T VisitFunctionStmt(Function statement);
    T VisitIfStmt(If statement);
    T VisitPrintStmt(Print statement);
    T VisitReturnStmt(Return statement);
    T VisitVarStmt(Var statement);
    T VisitWhileStmt(While statement);
}
