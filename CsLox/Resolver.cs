using System;
using System.Collections.Generic;
using System.Text;

namespace CsLox
{
    public class Resolver : AstNode.IVisitor<object>
    {
        private readonly Interpreter _interpreter;
        private readonly Stack<Dictionary<string, bool>> _scopes = new Stack<Dictionary<string, bool>>();
        private FunctionType _currentFuncType = FunctionType.NONE;

        private enum FunctionType
        {
            NONE,
            FUNCTION
        };

        public Resolver(Interpreter interpreter)
        {
            _interpreter = interpreter;
        }

        public void Resolve(List<AstNode> statements)
        {
            foreach (var statement in statements)
            {
                Resolve(statement);
            }
        }

        private void Resolve(AstNode stmt)
        {
            stmt.Accept(this);
        }

        private void ResolveLocal(AstNode expr, Token name)
        {
            var scopeIdx = 0;
            foreach (var scope in _scopes)
            {
                if (scope.ContainsKey(name.Lexeme))
                {
                    _interpreter.Resolve(expr, scopeIdx);
                }
                ++scopeIdx;
            }
        }

        private void ResolveFunction(FunctionStmt func, FunctionType type)
        {
            var enclosingFuncType = _currentFuncType;
            _currentFuncType = type;

            BeginScope();
            foreach (var param in func.Parameters)
            {
                Declare(param);
                Define(param);
            }
            Resolve(func.Body);
            EndScope();

            _currentFuncType = enclosingFuncType;
        }

        private void BeginScope()
        {
            _scopes.Push(new Dictionary<string, bool>());
        }

        private void EndScope()
        {
            _scopes.Pop();
        }

        private void Declare(Token name)
        {
            if (_scopes.Count == 0)
            {
                return;
            }

            var scope = _scopes.Peek();
            if (scope.ContainsKey(name.Lexeme))
            {
                Lox.Error(name, "Variable with this name already declared in this scope.");
            }

            scope.Add(name.Lexeme, false);
        }

        private void Define(Token name)
        {
            if (_scopes.Count == 0)
            {
                return;
            }

            _scopes.Peek().Add(name.Lexeme, true);
        }

        public object VisitAssignExpr(AssignExpr expr)
        {
            Resolve(expr.Value);
            ResolveLocal(expr, expr.Name);

            return null;
        }

        public object VisitBinaryExpr(BinaryExpr expr)
        {
            Resolve(expr.Left);
            Resolve(expr.Right);

            return null;
        }

        public object VisitBlockStmt(BlockStmt stmt)
        {
            BeginScope();
            Resolve(stmt.Statements);
            EndScope();

            return null;
        }

        public object VisitCallExpr(CallExpr expr)
        {
            Resolve(expr.Callee);
            foreach (var arg in expr.Arguments)
            {
                Resolve(arg);
            }

            return null;
        }

        public object VisitExpressionStmt(ExpressionStmt stmt)
        {
            Resolve(stmt.Expression);

            return null;
        }

        public object VisitFunctionStmt(FunctionStmt stmt)
        {
            // Defining the name before resolving the function
            // allows the function to refer to itself.
            Declare(stmt.Name);
            Define(stmt.Name);
            ResolveFunction(stmt, FunctionType.FUNCTION);

            return null;
        }

        public object VisitGroupingExpr(GroupingExpr expr)
        {
            Resolve(expr.Expression);

            return null;
        }

        public object VisitIfStmt(IfStmt stmt)
        {
            Resolve(stmt.Condition);
            Resolve(stmt.ThenBranch);
            if (stmt.ElseBranch != null)
            {
                Resolve(stmt.ElseBranch);
            }

            return null;
        }

        public object VisitLiteralExpr(LiteralExpr expr)
        {
            return null;
        }

        public object VisitLogicalExpr(LogicalExpr expr)
        {
            Resolve(expr.Left);
            Resolve(expr.Right);

            return null;
        }

        public object VisitPrintStmt(PrintStmt stmt)
        {
            Resolve(stmt.Expression);

            return null;
        }

        public object VisitReturnStmt(ReturnStmt stmt)
        {
            if (_currentFuncType == FunctionType.NONE)
            {
                Lox.Error(stmt.Keyword, "Cannot return from top-level code.");
            }

            if (stmt.Value != null)
            {
                Resolve(stmt.Value);
            }

            return null;
        }

        public object VisitUnaryExpr(UnaryExpr expr)
        {
            Resolve(expr.Right);

            return null;
        }

        public object VisitVarDeclStmt(VarDeclStmt stmt)
        {
            Declare(stmt.Name);
            if (stmt.Initializer != null)
            {
                Resolve(stmt.Initializer);
            }

            Define(stmt.Name);

            return null;
        }

        public object VisitVarExpr(VarExpr expr)
        {
            if (_scopes.Count > 0 && !_scopes.Peek()[expr.Name.Lexeme])
            {
                Lox.Error(expr.Name, "Cannot read local variable in its own initializer.");
            }

            ResolveLocal(expr, expr.Name);

            return null;
        }

        public object VisitWhileStmt(WhileStmt stmt)
        {
            Resolve(stmt.Condition);
            Resolve(stmt.Body);

            return null;
        }
    }
}
