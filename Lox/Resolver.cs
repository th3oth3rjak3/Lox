using Lox.Expressions;
using Lox.Statements;

namespace Lox;

public class Resolver(Interpreter interpreter) : IExprVisitor<Unit?>, IStmtVisitor<Unit?>
{
    private enum FunctionType
    {
        NONE,
        FUNCTION,
        INITIALIZER,
        METHOD,
    }

    private enum ClassType
    {
        NONE,
        CLASS,
        SUBCLASS,
    }

    private FunctionType CurrentFunction = FunctionType.NONE;
    private ClassType CurrentClass = ClassType.NONE;
    private List<Dictionary<string, bool>> scopes = [];

    private void BeginScope()
    {
        scopes.Add([]);
    }

    private void EndScope()
    {
        scopes.RemoveAt(scopes.Count - 1);
    }

    public void Resolve(List<Stmt> statements)
    {
        foreach (Stmt stmt in statements)
        {
            Resolve(stmt);
        }
    }

    private void Resolve(Stmt stmt)
    {
        stmt.Accept(this);
    }

    private void Resolve(Expr? expr)
    {
        if (expr is null) return;
        expr.Accept(this);
    }

    private void Declare(Token name)
    {
        if (scopes.Count == 0)
        {
            return;
        }

        var scope = scopes[^1];
        if (scope.ContainsKey(name.Lexeme))
        {
            Lox.Error(name, "Already a variable with this name in this scope.");
        }
        scope.Add(name.Lexeme, false);
    }

    private void Define(Token name)
    {
        if (scopes.Count == 0)
        {
            return;
        }

        var scope = scopes[^1];
        scope[name.Lexeme] = true;
    }

    private void ResolveLocal(Expr expression, Token token)
    {
        for (var i = scopes.Count - 1; i >= 0; i--)
        {
            if (scopes[i].ContainsKey(token.Lexeme))
            {
                interpreter.Resolve(expression, scopes.Count - 1 - i);
                return;
            }
        }
    }

    private void ResolveFunction(Function fn, FunctionType fnType)
    {
        var enclosingType = CurrentFunction;
        CurrentFunction = fnType;
        BeginScope();
        foreach (var param in fn.Parameters)
        {
            Declare(param);
            Define(param);
        }

        Resolve(fn.Body);
        EndScope();
        CurrentFunction = enclosingType;
    }

    public Unit? VisitBlockStmt(Block statement)
    {
        BeginScope();
        Resolve(statement.Statements);
        EndScope();
        return null;
    }

    public Unit? VisitVarStmt(Var statement)
    {
        Declare(statement.Token);
        if (statement.Initializer is not null)
        {
            Resolve(statement.Initializer);
        }

        Define(statement.Token);
        return null;
    }

    public Unit? VisitVariableExpr(Variable expression)
    {
        var name = expression.Token;
        if (scopes.Count > 0 && scopes[^1].ContainsKey(expression.Token?.Lexeme ?? "") && scopes[^1][name.Lexeme] == false)
        {
            Lox.Error(name, "Can't read local variable in its own initializer.");
        }

        ResolveLocal(expression, name);
        return null;
    }

    public Unit? VisitAssignExpr(Assign expression)
    {
        Resolve(expression.Value);
        ResolveLocal(expression, expression.Token);
        return null;
    }

    public Unit? VisitBinaryExpr(Binary expression)
    {
        Resolve(expression.Left);
        Resolve(expression.Right);
        return null;
    }

    public Unit? VisitCallExpr(Call expression)
    {
        Resolve(expression.Callee);
        foreach (var arg in expression.Arguments)
        {
            Resolve(arg);
        }
        return null;
    }

    public Unit? VisitGroupingExpr(Grouping expression)
    {
        Resolve(expression.Expression);
        return null;
    }

    public Unit? VisitLiteralExpr(Literal expression)
    {
        return null;
    }

    public Unit? VisitLogicalExpr(Logical expression)
    {
        Resolve(expression.Left);
        Resolve(expression.Right);
        return null;
    }

    public Unit? VisitUnaryExpr(Unary expression)
    {
        Resolve(expression.Right);
        return null;
    }

    public Unit? VisitExpressionStmt(Expression statement)
    {
        Resolve(statement.Expr);
        return null;
    }

    public Unit? VisitFunctionStmt(Function statement)
    {
        Declare(statement.Token);
        Define(statement.Token);

        ResolveFunction(statement, FunctionType.FUNCTION);
        return null;
    }

    public Unit? VisitIfStmt(If statement)
    {
        Resolve(statement.Condition);
        Resolve(statement.ThenBranch);
        if (statement.ElseBranch is not null)
        {
            Resolve(statement.ElseBranch);
        }
        return null;
    }

    public Unit? VisitPrintStmt(Print statement)
    {
        Resolve(statement.Expression);
        return null;
    }

    public Unit? VisitReturnStmt(Return statement)
    {
        if (CurrentFunction == FunctionType.NONE)
        {
            Lox.Error(statement.Keyword, "Can't return from top-level code.");
        }
        if (statement.Value is not null)
        {
            if (CurrentFunction == FunctionType.INITIALIZER)
            {
                Lox.Error(statement.Keyword, "Can't return a value from an initializer.");
            }
            Resolve(statement.Value);
        }
        return null;
    }

    public Unit? VisitWhileStmt(While statement)
    {
        Resolve(statement.Condition);
        Resolve(statement.Body);
        return null;
    }

    public Unit? VisitClassStmt(Class statement)
    {
        ClassType enclosingClass = CurrentClass;
        CurrentClass = ClassType.CLASS;

        Declare(statement.Name);
        Define(statement.Name);

        if (statement.Super is not null && statement.Name.Lexeme == statement.Super.Token.Lexeme)
        {
            Lox.Error(statement.Super.Token, "A class can't inherit from itself.");
        }

        if (statement.Super is not null)
        {
            CurrentClass = ClassType.SUBCLASS;
            Resolve(statement.Super);
        }

        if (statement.Super is not null)
        {
            BeginScope();
            scopes[^1]["super"] = true;
        }

        BeginScope();
        scopes[^1].Add("this", true);
        foreach (var method in statement.Methods)
        {
            var declaration = FunctionType.METHOD;
            if (method.Token.Lexeme == "init")
            {
                declaration = FunctionType.INITIALIZER;
            }
            ResolveFunction(method, declaration);
        }

        EndScope();
        if (statement.Super is not null)
        {
            EndScope();
        }
        CurrentClass = enclosingClass;
        return null;
    }

    public Unit? VisitGetExpr(Get expression)
    {
        Resolve(expression.Object);
        return null;
    }

    public Unit? VisitSetExpr(Set expression)
    {
        Resolve(expression.Value);
        Resolve(expression.Obj);
        return null;
    }

    public Unit? VisitThisExpr(This expression)
    {
        if (CurrentClass == ClassType.NONE)
        {
            Lox.Error(expression.Keyword, "Can't use 'this' outside of a class.");
        }
        ResolveLocal(expression, expression.Keyword);
        return null;
    }

    public Unit? VisitSuperExpr(Super expression)
    {
        if (CurrentClass == ClassType.NONE)
        {
            Lox.Error(expression.Keyword, "Can't use 'super' outside of a class.");
        }
        else if (CurrentClass != ClassType.SUBCLASS)
        {
            Lox.Error(expression.Keyword, "Can't use 'super' in a class with no superclass.");
        }
        ResolveLocal(expression, expression.Keyword);
        return null;
    }
}
