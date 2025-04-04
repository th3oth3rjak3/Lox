namespace Lox;

public class Parser(List<Token> tokens)
{
    private List<Token> Tokens { get; set; } = tokens;
    private int Current { get; set; }

    public List<Stmt> Parse()
    {
        List<Stmt> stmts = [];
        try
        {
            while (!IsAtEnd)
            {
                var stmt = HandleDeclaration();
                if (stmt != null) stmts.Add(stmt);
            }

        }
        catch (ParseError)
        {
            return stmts;
        }

        return stmts;
    }

    private Expr? HandleExpression()
    {
        return HandleEquality();
    }

    private Stmt? HandleDeclaration()
    {
        try
        {
            if (Match(TokenType.Var)) return HandleVarDeclaration();
            return HandleStatement();
        }
        catch (ParseError)
        {
            Synchronize();
            return null;
        }
    }

    private Var? HandleVarDeclaration()
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

    private Stmt HandleStatement() =>
        Match(TokenType.Print)
            ? HandlePrintStatement()
            : HandleExpressionStatement();


    private Print HandlePrintStatement()
    {
        var value = HandleExpression();
        Consume(TokenType.Semicolon, "Expect ';' after value.");
        return new Print(value);
    }
    private Expression HandleExpressionStatement()
    {
        var expr = HandleExpression();
        Consume(TokenType.Semicolon, "Expect ';' after expression.");
        return new Expression(expr);
    }

    private Expr? HandleEquality()
    {
        var expression = HandleComparison();
        while (Match(TokenType.BangEqual, TokenType.Equal))
        {
            var op = Previous();
            var right = HandleComparison();
            if (expression is null || op is null || right is null) return null;
            expression = new Binary(expression, op, right);
        }

        return expression;
    }

    private Expr? HandleComparison()
    {
        var expression = HandleTerm();
        while (Match(TokenType.Greater, TokenType.GreaterEqual, TokenType.Less, TokenType.LessEqual))
        {
            var op = Previous();
            var right = HandleTerm();
            if (expression is null || op is null || right is null) return null;
            expression = new Binary(expression, op, right);
        }

        return expression;
    }

    private Expr? HandleTerm()
    {
        var expression = HandleFactor();
        if (expression is null) return null;
        while (Match(TokenType.Minus, TokenType.Plus))
        {
            var op = Previous();
            if (op is null) return null;
            var right = HandleFactor();
            if (right is null) return null;
            expression = new Binary(expression, op, right);
        }

        return expression;
    }

    private Expr? HandleFactor()
    {
        var expression = HandleUnary();
        while (Match(TokenType.Slash, TokenType.Star))
        {
            var op = Previous();
            var right = HandleUnary();
            if (expression is null || op is null || right is null) return null;
            expression = new Binary(expression, op, right);
        }

        return expression;
    }

    private Expr? HandleUnary()
    {
        if (!Match(TokenType.Bang, TokenType.Minus)) return HandlePrimary();

        var op = Previous();
        var right = HandleUnary();
        if (op is null || right is null) return null;
        return new Unary(op, right);
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

    private Token? Consume(TokenType type, string message)
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
        if (!types.Any(Check)) return false;
        Advance();
        return true;
    }

    private bool Check(TokenType type) => !IsAtEnd && Peek().Type == type;

    private Token Peek() => Tokens[Current];

    private Token? Advance()
    {
        if (!IsAtEnd) Current++;
        return Previous();
    }

    private Token? Previous() =>
        Current == 0 ? null : Tokens[Current - 1];

    private bool IsAtEnd => Peek().Type == TokenType.Eof;
}