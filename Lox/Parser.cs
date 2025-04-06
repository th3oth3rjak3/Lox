using System.Diagnostics;
using System.Reflection;
using Lox.Errors;
using Lox.Expressions;
using Lox.Statements;

namespace Lox;

public class Parser(List<Token> tokens)
{
    private List<Token> Tokens { get; set; } = tokens;
    private int Current { get; set; }

    public List<Stmt> Parse()
    {
        List<Stmt> stmts = [];

        while (!IsAtEnd)
        {
            var stmt = HandleDeclaration();
            if (stmt != null) stmts.Add(stmt);
        }

        return stmts;
    }

    private Expr HandleExpression()
    {
        return HandleAssignment();
    }

    private Expr HandleAssignment()
    {
        var expr = HandleOr();
        if (Match(TokenType.Equal))
        {
            var equals = Previous();
            var value = HandleAssignment();

            if (expr is Variable v)
            {
                var name = v.Token ?? throw new RuntimeError(equals, "Variable name cannot be empty.");
                return new Assign(name, value);
            }

            Error(equals, "Invalid assignment target.");
        }

        return expr;
    }

    private Expr HandleOr()
    {
        var expr = HandleAnd();

        while (Match(TokenType.Or))
        {
            var op = Previous();
            var right = HandleAnd();
            expr = new Logical(expr, op, right);
        }

        return expr;
    }

    private Expr HandleAnd()
    {
        var expr = HandleEquality();

        while (Match(TokenType.And))
        {
            var op = Previous();
            var right = HandleEquality();
            expr = new Logical(expr, op, right);
        }

        return expr;
    }

    private Stmt? HandleDeclaration()
    {
        try
        {
            if (Match(TokenType.Fun)) return HandleFunction("function");
            if (Match(TokenType.Var)) return HandleVarDeclaration();
            return HandleStatement();
        }
        catch (ParseError)
        {
            Synchronize();
            return null;
        }
    }

    private Var HandleVarDeclaration()
    {
        var name = Consume(TokenType.Identifier, "Expect variable name.");
        Expr? initializer = null;
        if (Match(TokenType.Equal))
        {
            initializer = HandleExpression();
        }

        Consume(TokenType.Semicolon, "Expect ';' after variable declaration.");
        return new Var(name, initializer);
    }

    private Stmt HandleWhileStatement()
    {
        Consume(TokenType.LeftParen, "Expect '(' after 'while'.");
        var condition = HandleExpression();
        Consume(TokenType.RightParen, "Expect ')' after condition.");

        var body = HandleStatement();

        return new While(condition, body);
    }

    private Stmt HandleStatement()
    {
        if (Match(TokenType.For)) return HandleForStatement();
        if (Match(TokenType.If)) return HandleIfStatement();
        if (Match(TokenType.Print)) return HandlePrintStatement();
        if (Match(TokenType.Return)) return HandleReturnStatement();
        if (Match(TokenType.While)) return HandleWhileStatement();
        if (Match(TokenType.LeftBrace)) return new Block(HandleBlock());

        return HandleExpressionStatement();
    }

    private Stmt HandleForStatement()
    {
        Consume(TokenType.LeftParen, "Expect '(' after 'for'.");

        // Initializer like var i + 1;
        Stmt? initializer;
        if (Match(TokenType.Semicolon))
        {
            initializer = null;
        }
        else if (Match(TokenType.Var))
        {
            initializer = HandleVarDeclaration();
        }
        else
        {
            initializer = HandleExpressionStatement();
        }

        // Condition like i < 10;
        Expr? condition = null;
        if (!Check(TokenType.Semicolon))
        {
            condition = HandleExpression();
        }

        Consume(TokenType.Semicolon, "Expect ';' after loop condition.");

        // Increment like i = i + 1;
        Expr? increment = null;
        if (!Check(TokenType.RightParen))
        {
            increment = HandleExpression();
        }

        Consume(TokenType.RightParen, "Expect ')' after for clauses.");

        var body = HandleStatement();

        if (increment is not null)
        {
            List<Stmt> statements = [
                body,
                new Expression(increment),
            ];
            body = new Block(statements);
        }

        if (condition is null)
        {
            condition = new Literal(true);
        }

        body = new While(condition, body);

        if (initializer is not null)
        {
            List<Stmt> statements = [
                initializer, body
            ];
            body = new Block(statements);
        }

        return body;
    }

    private Stmt HandleIfStatement()
    {
        Consume(TokenType.LeftParen, "Expect '(' after 'if'.");
        var condition = HandleExpression();
        Consume(TokenType.RightParen, "Expect ')' after if condition.");

        var thenBranch = HandleStatement();
        Stmt? elseBranch = null;
        if (Match(TokenType.Else))
        {
            elseBranch = HandleStatement();
        }

        return new If(condition, thenBranch, elseBranch);
    }

    private List<Stmt> HandleBlock()
    {
        List<Stmt> stmts = [];

        while (!Check(TokenType.RightBrace) && !IsAtEnd)
        {
            var declaration = HandleDeclaration();
            if (declaration is null) return [];
            stmts.Add(declaration);
        }

        Consume(TokenType.RightBrace, "Expect '}' after block.");
        return stmts;
    }


    private Print HandlePrintStatement()
    {
        var value = HandleExpression();
        Consume(TokenType.Semicolon, "Expect ';' after value.");
        return new Print(value);
    }

    private Stmt HandleReturnStatement()
    {
        var keyword = Previous();
        Expr? value = null;
        if (!Check(TokenType.Semicolon))
        {
            value = HandleExpression();
        }

        Consume(TokenType.Semicolon, "Expect ';' after return value.");
        return new Return(keyword, value);
    }


    private Expression HandleExpressionStatement()
    {
        var expr = HandleExpression();
        Consume(TokenType.Semicolon, "Expect ';' after expression.");
        return new Expression(expr);
    }

    private Function HandleFunction(string kind)
    {
        var token = Consume(TokenType.Identifier, $"Expect {kind} name.");
        Consume(TokenType.LeftParen, $"Expect '(' after {kind} name.");
        List<Token> parameters = [];
        if (!Check(TokenType.RightParen))
        {
            do
            {
                if (parameters.Count >= 255)
                {
                    Error(Peek(), "Can't have more than 255 parameters.");
                }
                parameters.Add(Consume(TokenType.Identifier, "Expect parameter name."));
            } while (Match(TokenType.Comma));
        }

        Consume(TokenType.RightParen, "Expect ')' after parameters.");

        Consume(TokenType.LeftBrace, $"Expect '{{' before {kind} body.");
        List<Stmt> body = HandleBlock();
        return new Function(token, parameters, body);
    }

    private Expr HandleEquality()
    {
        var expression = HandleComparison();
        while (Match(TokenType.BangEqual, TokenType.EqualEqual))
        {
            var op = Previous();
            var right = HandleComparison();
            expression = new Binary(expression, op, right);
        }

        return expression;
    }

    private Expr HandleComparison()
    {
        var expression = HandleTerm();
        while (Match(TokenType.Greater, TokenType.GreaterEqual, TokenType.Less, TokenType.LessEqual))
        {
            var op = Previous();
            var right = HandleTerm();
            expression = new Binary(expression, op, right);
        }

        return expression;
    }

    private Expr HandleTerm()
    {
        var expression = HandleFactor();
        while (Match(TokenType.Minus, TokenType.Plus))
        {
            var op = Previous();
            var right = HandleFactor();
            expression = new Binary(expression, op, right);
        }

        return expression;
    }

    private Expr HandleFactor()
    {
        var expression = HandleUnary();
        while (Match(TokenType.Slash, TokenType.Star))
        {
            var op = Previous();
            var right = HandleUnary();
            expression = new Binary(expression, op, right);
        }

        return expression;
    }

    private Expr HandleUnary()
    {
        if (!Match(TokenType.Bang, TokenType.Minus)) return HandleCall();

        var op = Previous();
        var right = HandleUnary();
        return new Unary(op, right);
    }

    private Expr HandleCall()
    {
        var expr = HandlePrimary();

        while (true)
        {
            if (Match(TokenType.LeftParen))
            {
                expr = FinishCall(expr);
            }
            else
            {
                break;
            }
        }

        return expr;
    }

    private Expr FinishCall(Expr callee)
    {
        List<Expr> args = [];
        if (!Check(TokenType.RightParen))
        {
            do
            {
                if (args.Count >= 255)
                {
                    Error(Peek(), "Can't have more than 255 arguments.");
                }
                args.Add(HandleExpression());
            } while (Match(TokenType.Comma));
        }

        var paren = Consume(TokenType.RightParen, "Expect ')' after arguments.");

        return new Call(callee, paren, args);
    }

    private Expr HandlePrimary()
    {
        if (Match(TokenType.False)) return new Literal(false);
        if (Match(TokenType.True)) return new Literal(true);
        if (Match(TokenType.Nil)) return new Literal(null);

        if (Match(TokenType.Number, TokenType.String))
        {
            return new Literal(Previous()?.Literal);
        }

        if (Match(TokenType.Identifier)) return new Variable(Previous());

        if (Match(TokenType.LeftParen))
        {
            var expression = HandleExpression();
            Consume(TokenType.RightParen, "Expect ')' after expression.");
            return new Grouping(expression);
        }

        throw Error(Peek(), "Expected expression.");
    }

    private Token Consume(TokenType type, string message)
    {
        if (Check(type)) return Advance();
        throw Error(Peek(), message);
    }

    private static ParseError Error(Token token, string message)
    {
        Lox.Error(token, message);
        return new ParseError();
    }

    private void Synchronize()
    {
        Advance();

        while (!IsAtEnd)
        {
            if (Previous()?.Type == TokenType.Semicolon) return;

            switch (Peek().Type)
            {
                case TokenType.Class:
                case TokenType.Fun:
                case TokenType.Var:
                case TokenType.For:
                case TokenType.If:
                case TokenType.While:
                case TokenType.Print:
                case TokenType.Return:
                    return;
            }

            Advance();
        }
    }



    private bool Match(params TokenType[] types)
    {
        foreach (var type in types)
        {
            if (Check(type))
            {
                Advance();
                return true;
            }
        }

        return false;
    }

    private bool Check(TokenType type) => !IsAtEnd && Peek().Type == type;

    private Token Peek() => Tokens[Current];

    private Token Advance()
    {
        if (!IsAtEnd) Current++;
        return Previous();
    }

    private Token Previous() =>
        Tokens[Current - 1];

    private bool IsAtEnd => Peek().Type == TokenType.Eof;
}