using System.Text.RegularExpressions;

namespace Lox;

public class Parser(List<Token> tokens)
{
    private List<Token> Tokens { get; set; } = tokens;
    private int Current { get; set; } = 0;

    public Expression? Parse()
    {
        try
        {
            return HandleExpression();
        }
        catch (ParseError e)
        {
            return null;
        }
    }
    
    private Expression HandleExpression()
    {
        return HandleEquality();
    }

    private Expression HandleEquality()
    {
        var expression = HandleComparison();
        while (Match(TokenType.BangEqual, TokenType.Equal))
        {
            var op = Previous();
            var right = HandleComparison();
            expression = new Binary(expression, op, right);
        }
        
        return expression;
    }

    private Expression HandleComparison()
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

    private Expression HandleTerm()
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

    private Expression HandleFactor()
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

    private Expression HandleUnary()
    {
        if (!Match(TokenType.Bang, TokenType.Minus)) return HandlePrimary();
        
        var op = Previous();
        var right = HandleUnary();
        return new Unary(op, right);
    }

    private Expression HandlePrimary()
    {
        if (Match(TokenType.False)) return new Literal(false);
        if (Match(TokenType.True)) return new Literal(true);
        if (Match(TokenType.Nil)) return new Literal(null);

        if (Match(TokenType.Number, TokenType.String))
        {
            return new Literal(Previous().Literal);
        }

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
        if (!types.Any(Check)) return false;
        Advance();
        return true;
    }

    private bool Check(TokenType type) => !IsAtEnd && Peek().Type == type;

    private Token Peek() => Tokens[Current];
    
    private Token Advance()
    {
        if (!IsAtEnd) Current++;
        return Previous();
    }
    
    private Token Previous() => 
        Current == 0 ? null : Tokens[Current - 1];

    private bool IsAtEnd => Peek().Type == TokenType.Eof;
}