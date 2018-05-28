using System;
using System.Collections.Generic;

namespace CsLox
{
    class ParseException : Exception
    {
    }

    public class Parser
    {
        private static int MAX_ARG_COUNT = 8;

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

        public List<AstNode> Parse()
        {
            var statements = new List<AstNode>();
            try
            {
                while (!IsAtEnd())
                {
                    statements.Add(Declaration());
                }
                return statements;
            }
            catch (ParseException)
            {
                return null;
            }
        }

        // Declaration -> VarDeclaration | Statement
        private AstNode Declaration()
        {
            try
            {
                if (Match(TokenType.VAR))
                {
                    return VarDeclaration();
                }

                return Statement();
            }
            catch (ParseException)
            {
                Synchronize();
                return null;
            }
        }

        // Statement -> ExpressionStatement | PrintStatement | Block
        private AstNode Statement()
        {
            if (Match(TokenType.FOR))
            {
                return ForStatement();
            }
            if (Match(TokenType.IF))
            {
                return IfStatement();
            }
            if (Match(TokenType.WHILE))
            {
                return WhileStatement();
            }
            if (Match(TokenType.PRINT))
            {
                return PrintStatement();
            }
            if (Match(TokenType.LEFT_BRACE))
            {
                return new BlockStmt(Block());
            }

            return ExpressionStatement();
        }

        // Block -> "{" Declaration* "}"
        private List<AstNode> Block()
        {
            var statements = new List<AstNode>();
            
            while (!Check(TokenType.RIGHT_BRACE) && !IsAtEnd())
            {
                statements.Add(Declaration());
            }

            Consume(TokenType.RIGHT_BRACE, "Expect '}' after block.");

            return statements;
        }

        // ExpressionStatement -> Expression ";"
        private AstNode ExpressionStatement()
        {
            AstNode expr = Expression();
            Consume(TokenType.SEMICOLON, "Expect ';' ater expression.");

            return new ExpressionStmt(expr);
        }

        // ForStatement -> "for" "(" ( varDecl | ExpressionStmt | ";" ) Expression? ";" Expression? ";" ")" Statement
        private AstNode ForStatement()
        {
            Consume(TokenType.LEFT_PAREN, "Expect '(' after 'for'.");
            AstNode initializer = default(AstNode);
            if (!Match(TokenType.SEMICOLON))
            {
                initializer = Match(TokenType.VAR)
                    ? VarDeclaration()
                    : ExpressionStatement();
            }

            AstNode condition = Check(TokenType.SEMICOLON) ? new LiteralExpr(true) : Expression();
            Consume(TokenType.SEMICOLON, "Expect ';' after 'for' loop condition.");

            AstNode increment = Check(TokenType.RIGHT_PAREN) ? null : Expression();
            Consume(TokenType.RIGHT_PAREN, "Expect ')' after for clauses.");

            AstNode body = Statement();

            // Desugarize for loop
            // From: for (var i = 0; i < 10; ++i) doStuff();
            // To: var i = 0; while (i < 10) { doStuff(); i = i+1; }
            if (increment != null)
            {
                body = new BlockStmt(new List<AstNode> { body, new ExpressionStmt(increment) });
            }

            body = new WhileStmt(condition, body);

            if (initializer != null)
            {
                body = new BlockStmt(new List<AstNode> { initializer, body });
            }

            return body;
        }

        // IfStatement -> "if" "(" Expression ")" Statement ( "else" Statement )?
        private AstNode IfStatement()
        {
            Consume(TokenType.LEFT_PAREN, "Expect '(' after 'if'.");
            AstNode condition = Expression();
            Consume(TokenType.RIGHT_PAREN, "Expect ')' after if condition.");

            AstNode thenBranch = Statement();
            AstNode elseBranch = null;
            if (Match(TokenType.ELSE))
            {
                elseBranch = Statement();
            }

            return new IfStmt(condition, thenBranch, elseBranch);
        }

        // WhileStatement -> "while" "(" Expression ")" Statement
        private AstNode WhileStatement()
        {
            Consume(TokenType.LEFT_PAREN, "Expect '(' after 'while'.");
            AstNode condition = Expression();
            Consume(TokenType.RIGHT_PAREN, "Expect ')' after while condition.");
            AstNode body = Statement();

            return new WhileStmt(condition, body);
        }

        // PrintStatement -> "print" Expression ";"
        private AstNode PrintStatement()
        {
            AstNode value = Expression();
            Consume(TokenType.SEMICOLON, "Expect ';' after value.");

            return new PrintStmt(value);
        }

        // VarDeclaration -> "var" IDENTIFIER ( "=" Expression )? ";"
        private AstNode VarDeclaration()
        {
            Token name = Consume(TokenType.IDENTIFIER, "Expect variable name.");
            AstNode initializer = null;
            if (Match(TokenType.EQUAL))
            {
                initializer = Expression();
            }

            Consume(TokenType.SEMICOLON, "Expect ';' after variable declaration.");

            return new VarDeclStmt(name, initializer);
        }

        // Expression -> Assignment
        private AstNode Expression()
        {
            return Assignment();
        }

        // Assignment -> IDENTIFIER "=" Assignment
        //             | LogicalOr
        private AstNode Assignment()
        {
            AstNode expr = LogicalOr();

            if (Match(TokenType.EQUAL))
            {
                Token equals = Previous();
                AstNode value = Assignment();

                if (expr is VarExpr)
                {
                    Token name = (expr as VarExpr).Name;
                    return new AssignExpr(name, value);
                }

                throw Error(equals, "Invalid assignment target.");
            }

            return expr;
        }

        // LogicalOr -> LogicalAnd ( "or" LogicalAnd )*
        private AstNode LogicalOr()
        {
            AstNode expr = LogicalAnd();

            while (Match(TokenType.OR))
            {
                Token oper = Previous();
                AstNode right = LogicalAnd();
                expr = new LogicalExpr(expr, oper, right);
            }

            return expr;
        }

        // LogicalAnd -> Equality ( "and" Equality )*
        private AstNode LogicalAnd()
        {
            AstNode expr = Equality();

            while (Match(TokenType.AND))
            {
                Token oper = Previous();
                AstNode right = Equality();
                expr = new LogicalExpr(expr, oper, right);
            }

            return expr;
        }

        // Equality -> Comparison ( ( != | == ) Comparison )*
        private AstNode Equality()
        {
            AstNode expr = Comparison();

            while (Match(TokenType.BANG_EQUAL, TokenType.EQUAL_EQUAL))
            {
                Token oper = Previous();
                AstNode right = Comparison();
                expr = new BinaryExpr(expr, oper, right);
            }

            return expr;
        }

        // Comparison -> Addition ( ( < | <= | >= | > ) Addition )*
        private AstNode Comparison()
        {
            AstNode expr = Addition();

            while (Match(TokenType.GREATER, TokenType.GREATER_EQUAL, TokenType.LESS, TokenType.LESS_EQUAL))
            {
                Token oper = Previous();
                AstNode right = Addition();
                expr = new BinaryExpr(expr, oper, right);
            }

            return expr;
        }

        // Addition -> Multiplication ( ( + | - ) Multiplication )*
        private AstNode Addition()
        {
            AstNode expr = Multiplication();

            while (Match(TokenType.MINUS, TokenType.PLUS))
            {
                Token oper = Previous();
                AstNode right = Multiplication();
                expr = new BinaryExpr(expr, oper, right);
            }

            return expr;
        }

        // Multiplication -> Unary ( ( / | * ) Unary )*
        private AstNode Multiplication()
        {
            AstNode expr = Unary();

            while (Match(TokenType.SLASH, TokenType.STAR))
            {
                Token oper = Previous();
                AstNode right = Unary();
                expr = new BinaryExpr(expr, oper, right);
            }

            return expr;
        }

        // Unary -> ( ! | - ) Unary
        //        | Call
        private AstNode Unary()
        {
            if (Match(TokenType.BANG, TokenType.MINUS))
            {
                Token oper = Previous();
                AstNode right = Unary();
                return new UnaryExpr(oper, right);
            }

            return Call();
        }

        // Call -> Primary ( "(" Arguments? ")" )*
        private AstNode Call()
        {
            AstNode expr = Primary();
            while (Match(TokenType.LEFT_PAREN))
            {
                expr = FinishCall(expr);
            }

            return expr;
        }

        private AstNode FinishCall(AstNode callee)
        {
            var arguments = new List<AstNode>();
            if (!Check(TokenType.RIGHT_PAREN))
            {
                do
                {
                    if (arguments.Count >= MAX_ARG_COUNT)
                    {
                        Error(Peek(), $"Cannot have more than {MAX_ARG_COUNT} arguments.");
                    }
                    arguments.Add(Expression());
                } while (Match(TokenType.COMMA));
            }

            Token paren = Consume(TokenType.RIGHT_PAREN, "Expect ')' after arguments.");

            return new CallExpr(callee, paren, arguments);
        }

        // Primary -> NUMBER | STRING
        //          | "false" | "true" | "nil"
        //          | "(" Expression ")"
        //          | IDENTIFIER
        private AstNode Primary()
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
            if (Match(TokenType.IDENTIFIER))
            {
                return new VarExpr(Previous());
            }
            if (Match(TokenType.LEFT_PAREN))
            {
                AstNode expr = Expression();
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
