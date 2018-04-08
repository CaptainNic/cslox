using System;
using System.Collections.Generic;

namespace CsLox
{
    class ParseException : Exception
    {
    }

    public class Parser
    {
        private readonly List<Token> _tokens;
        private int _current = 0;

        /// <summary>
        /// This parser is implemented as a bottom-up parser.
        /// It attempts to match the lowest precedent operator and moves
        /// up to higher precendent ones until it finds a match.
        /// </summary>
        /// <param name="tokens"></param>
        public Parser(List<Token> tokens)
        {
            _tokens = tokens;
        }

        public Expr Parse()
        {
            try
            {
                return Expression();
            }
            catch (ParseException)
            {
                return null;
            }
        }

        // Expression -> Equality
        private Expr Expression()
        {
            return Equality();
        }

        // Equality -> Comparison ( ( != | == ) Comparison )*
        private Expr Equality()
        {
            Expr expr = Comparison();

            while (Match(TokenType.BANG_EQUAL, TokenType.EQUAL_EQUAL))
            {
                Token oper = Previous();
                Expr right = Comparison();
                expr = new BinaryExpr(expr, oper, right);
            }

            return expr;
        }

        // Comparison -> Addition ( ( < | <= | >= | > ) Addition )*
        private Expr Comparison()
        {
            Expr expr = Addition();

            while (Match(TokenType.GREATER, TokenType.GREATER_EQUAL, TokenType.LESS, TokenType.LESS_EQUAL))
            {
                Token oper = Previous();
                Expr right = Addition();
                expr = new BinaryExpr(expr, oper, right);
            }

            return expr;
        }

        // Addition -> Multiplication ( ( + | - ) Multiplication )*
        private Expr Addition()
        {
            Expr expr = Multiplication();

            while (Match(TokenType.MINUS, TokenType.PLUS))
            {
                Token oper = Previous();
                Expr right = Multiplication();
                expr = new BinaryExpr(expr, oper, right);
            }

            return expr;
        }

        // Multiplication -> Unary ( ( / | * ) Unary )*
        private Expr Multiplication()
        {
            Expr expr = Unary();

            while (Match(TokenType.SLASH, TokenType.STAR))
            {
                Token oper = Previous();
                Expr right = Unary();
                expr = new BinaryExpr(expr, oper, right);
            }

            return expr;
        }

        // Unary -> ( ! | - ) Unary
        //        | Primary
        private Expr Unary()
        {
            if (Match(TokenType.BANG, TokenType.MINUS))
            {
                Token oper = Previous();
                Expr right = Unary();
                return new UnaryExpr(oper, right);
            }

            return Primary();
        }

        // Primary -> NUMBER | STRING
        //          | "false" | "true" | "nil"
        //          | "(" Expression ")"
        private Expr Primary()
        {
            if (Match(TokenType.NUMBER, TokenType.STRING))
            {
                return new LiteralExpr(Previous().Literal);
            }
            if (Match(TokenType.FALSE))
            {
                return new LiteralExpr(false);
            }
            if (Match(TokenType.TRUE))
            {
                return new LiteralExpr(true);
            }
            if (Match(TokenType.NIL))
            {
                return new LiteralExpr(null);
            }
            if (Match(TokenType.LEFT_PAREN))
            {
                Expr expr = Expression();
                Consume(TokenType.RIGHT_PAREN, "Expected ')' after expression.");
                return new GroupingExpr(expr);
            }

            throw Error(Peek(), "Expected expression.");
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

        private Token Consume(TokenType type, string errorMessage)
        {
            if (Check(type))
            {
                return Advance();
            }

            throw Error(Peek(), errorMessage);
        }

        private bool Check(TokenType type)
        {
            if (IsAtEnd())
            {
                return false;
            }

            return Peek().Type == type;
        }

        private Token Advance()
        {
            if (!IsAtEnd())
            {
                _current++;
            }

            return Previous();
        }

        private bool IsAtEnd()
        {
            return Peek().Type == TokenType.EOF;
        }

        private Token Peek()
        {
            return _tokens[_current];
        }

        private Token Previous()
        {
            return _tokens[_current - 1];
        }

        private ParseException Error(Token token, string message)
        {
            Lox.Error(token, message);
            return new ParseException();
        }

        /// <summary>
        /// Attempts to put the parser back into a valid state by
        /// consuming tokens until it find the start of the next statement.
        /// </summary>
        private void Synchronize()
        {
            Advance();

            while (!IsAtEnd())
            {
                if (Previous().Type == TokenType.SEMICOLON)
                {
                    return;
                }

                switch (Peek().Type)
                {
                    case TokenType.CLASS:
                    case TokenType.FUN:
                    case TokenType.VAR:
                    case TokenType.FOR:
                    case TokenType.IF:
                    case TokenType.WHILE:
                    case TokenType.PRINT:
                    case TokenType.RETURN:
                        return;
                }

                Advance();
            }
        }
    }
}
