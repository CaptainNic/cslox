using System;
using System.Collections.Generic;

namespace CsLox
{
    public class LoxRuntimeException : Exception
    {
        public readonly Token Token;

        public LoxRuntimeException(Token token, string message)
            : base(message)
        {
            Token = token;
        }

        public LoxRuntimeException(Token token, string message, Exception innerException)
            : base(message, innerException)
        {
            Token = token;
        }
    }

    public class Interpreter : AstNode.IVisitor<object>
    {
        private readonly Scope _global = new Scope();
        private Scope _scope;

        public Interpreter()
        {
            _scope = _global;
            DefineBuiltIns();
        }

        public Scope GlobalScope => _global;

        public void Interpret(List<AstNode> statements)
        {
            try
            {
                foreach (var stmt in statements)
                {
                    Execute(stmt);
                }
            }
            catch (LoxRuntimeException ex)
            {
                Lox.RuntimeError(ex);
            }
        }

        private void DefineBuiltIns()
        {
            _global.Define(new BuiltIns.Clock());
        }

        private void Execute(AstNode stmt)
        {
            stmt.Accept(this);
        }

        public void ExecuteBlock(List<AstNode> statements, Scope scope)
        {
            // Temporarily store the current scope for restoration
            // after the block is finished executing.
            Scope parentScope = _scope;
            try
            {
                _scope = scope;
                foreach (var stmt in statements)
                {
                    Execute(stmt);
                }
            }
            finally
            {
                _scope = parentScope;
            }
        }

        private object Evaluate(AstNode expr)
        {
            return expr.Accept(this);
        }

        public object VisitBlockStmt(BlockStmt stmt)
        {
            ExecuteBlock(stmt.Statements, new Scope(_scope));

            return null;
        }

        public object VisitExpressionStmt(ExpressionStmt stmt)
        {
            Evaluate(stmt.Expression);

            return null;
        }

        public object VisitFunctionStmt(FunctionStmt stmt)
        {
            var func = new LoxFunction(stmt, _scope);
            _scope.Define(stmt.Name, func);

            return null;
        }

        public object VisitIfStmt(IfStmt stmt)
        {
            if (IsTruthy(Evaluate(stmt.Condition)))
            {
                Execute(stmt.ThenBranch);
            }
            else if (stmt.ElseBranch != null)
            {
                Execute(stmt.ElseBranch);
            }

            return null;
        }

        public object VisitWhileStmt(WhileStmt stmt)
        {
            while (IsTruthy(Evaluate(stmt.Condition)))
            {
                Execute(stmt.Body);
            }

            return null;
        }

        public object VisitReturnStmt(ReturnStmt stmt)
        {
            throw new Return(stmt.Value != null 
                ? Evaluate(stmt.Value)
                : null);
        }

        public object VisitPrintStmt(PrintStmt stmt)
        {
            object value = Evaluate(stmt.Expression);
            Console.WriteLine(value ?? "nil");

            return null;
        }

        public object VisitVarDeclStmt(VarDeclStmt stmt)
        {
            object value = null;
            if (stmt.Initializer != null)
            {
                value = Evaluate(stmt.Initializer);
            }

            _scope.Define(stmt.Name, value);

            return null;
        }

        public object VisitAssignExpr(AssignExpr expr)
        {
            object value = Evaluate(expr.Value);

            _scope.Assign(expr.Name, value);

            return value;
        }

        public object VisitBinaryExpr(BinaryExpr expr)
        {
            object left = Evaluate(expr.Left);
            object right = Evaluate(expr.Right);

            switch (expr.Oper.Type)
            {
                case TokenType.PLUS:
                    if (left is double && right is double)
                    {
                        return (double)left + (double)right;
                    }
                    if (left is string && right is string)
                    {
                        return $"{left}{right}";
                    }
                    throw new LoxRuntimeException(expr.Oper, $"Operands must both be numbers or strings.");
                case TokenType.MINUS:
                    AssertNumberType(expr.Oper, left, right);
                    return (double)left - (double)right;
                case TokenType.SLASH:
                    AssertNumberType(expr.Oper, left, right);
                    return (double)left / (double)right;
                case TokenType.STAR:
                    AssertNumberType(expr.Oper, left, right);
                    return (double)left * (double)right;
                case TokenType.LESS:
                    AssertNumberType(expr.Oper, left, right);
                    return (double)left < (double)right;
                case TokenType.LESS_EQUAL:
                    AssertNumberType(expr.Oper, left, right);
                    return (double)left <= (double)right;
                case TokenType.GREATER:
                    AssertNumberType(expr.Oper, left, right);
                    return (double)left > (double)right;
                case TokenType.GREATER_EQUAL:
                    AssertNumberType(expr.Oper, left, right);
                    return (double)left >= (double)right;
                case TokenType.BANG_EQUAL:
                    return !IsEqual(left, right);
                case TokenType.EQUAL_EQUAL:
                    return IsEqual(left, right);
            }

            // Should be unreachable.
            throw new LoxRuntimeException(expr.Oper, $"BinaryExpr has unexpected operator: {expr.Oper.Type}.");
        }

        public object VisitCallExpr(CallExpr expr)
        {
            var callee = Evaluate(expr.Callee);

            var args = new List<object>();
            foreach (var arg in expr.Arguments)
            {
                args.Add(Evaluate(arg));
            }

            var func = callee as ICallable;
            if (func == null)
            {
                throw new LoxRuntimeException(expr.Paren, "Can only call functions and classes.");
            }

            if (args.Count != func.Arity)
            {
                throw new LoxRuntimeException(expr.Paren, $"Expected {func.Arity} arguments, but got {args.Count}.");
            }

            return func.Call(this, args);
        }

        public object VisitGroupingExpr(GroupingExpr expr)
        {
            return Evaluate(expr.Expression);
        }

        public object VisitLiteralExpr(LiteralExpr expr)
        {
            return expr.Value;
        }

        public object VisitLogicalExpr(LogicalExpr expr)
        {
            object left = Evaluate(expr.Left);

            // Short-circuiting logic
            if (expr.Oper.Type == TokenType.OR)
            {
                if (IsTruthy(left))
                {
                    return left;
                }
            }
            else
            {
                if (!IsTruthy(left))
                {
                    return left;
                }
            }

            return Evaluate(expr.Right);
        }

        public object VisitUnaryExpr(UnaryExpr expr)
        {
            object right = Evaluate(expr.Right);

            switch (expr.Oper.Type)
            {
                case TokenType.MINUS:
                    AssertNumberType(expr.Oper, right);
                    return -(double)right;
                case TokenType.BANG:
                    return !IsTruthy(right);
            }

            // Should be unreachable.
            throw new LoxRuntimeException(expr.Oper, $"UnaryExpr has unexpected operator: {expr.Oper.Type}.");
        }

        public object VisitVarExpr(VarExpr expr)
        {
            return _scope.Get(expr.Name);
        }

        private bool IsTruthy(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (obj is bool)
            {
                return (bool)obj;
            }

            return true;
        }

        private bool IsEqual(object a, object b)
        {
            if (a == null && b == null)
            {
                return true;
            }

            if (a == null)
            {
                return false;
            }

            return a.Equals(b);
        }

        private void AssertNumberType(Token oper, object operand)
        {
            if (operand is double)
            {
                return;
            }

            throw new LoxRuntimeException(oper, "Operand must be a number.");
        }

        private void AssertNumberType(Token oper, params object[] operands)
        {
            try
            {
                foreach (var operand in operands)
                {
                    AssertNumberType(oper, operand);
                }
            }
            catch (Exception ex)
            {
                throw new LoxRuntimeException(oper, "Operands must be numbers.", ex);
            }
            
        }
    }
}
