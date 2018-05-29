using System;
using System.Linq;
using System.Text;

namespace CsLox
{
    public class AstPrinter : AstNode.IVisitor<string>
    {
        public string Print(AstNode node)
        {
            return node.Accept(this);
        }

        public string VisitBlockStmt(BlockStmt stmt)
        {
            var sb = new StringBuilder();
            sb.AppendLine("(block");
            foreach (var statement in stmt.Statements)
            {
                sb.AppendLine($"    {Parenthesize("", statement)}");
            }
            sb.Append(")");

            return sb.ToString();
        }

        public string VisitExpressionStmt(ExpressionStmt stmt)
        {
            return Parenthesize("exprStmt", stmt.Expression);
        }

        public string VisitFunctionStmt(FunctionStmt stmt)
        {
            var sb = new StringBuilder();
            sb.Append($"(fundecl {stmt.Name.Lexeme}(");
            sb.Append(string.Join(",", stmt.Parameters.Select(x => x.Lexeme)));
            sb.AppendLine(") {");
            foreach (var node in stmt.Body)
            {
                sb.AppendLine($"    {node.Accept(this)}");
            }
            sb.AppendLine("}");

            return sb.ToString();
        }

        public string VisitIfStmt(IfStmt stmt)
        {
            if (stmt.ElseBranch != null)
            {
                return Parenthesize("if", stmt.Condition, stmt.ThenBranch, stmt.ElseBranch);
            }
            else
            {
                return Parenthesize("if", stmt.Condition, stmt.ThenBranch);
            }
        }

        public string VisitWhileStmt(WhileStmt stmt)
        {
            return Parenthesize("while", stmt.Condition, stmt.Body);
        }

        public string VisitReturnStmt(ReturnStmt stmt)
        {
            return Parenthesize("return", stmt.Value);
        }

        public string VisitPrintStmt(PrintStmt stmt)
        {
            return Parenthesize("print", stmt.Expression);
        }

        public string VisitVarDeclStmt(VarDeclStmt stmt)
        {
            return Parenthesize($"declvar {stmt.Name.Lexeme}", stmt.Initializer);
        }

        public string VisitAssignExpr(AssignExpr expr)
        {
            return Parenthesize($"{expr.Name.Lexeme} = ", expr.Value);
        }

        public string VisitBinaryExpr(BinaryExpr expr)
        {
            return Parenthesize(expr.Oper.Lexeme, expr.Left, expr.Right);
        }

        public string VisitCallExpr(CallExpr expr)
        {
            var sb = new StringBuilder();
            sb.Append($"(call {expr.Callee.Accept(this)}(");
            foreach (var arg in expr.Arguments)
            {
                sb.Append($"{arg.Accept(this)}, ");
            }
            sb.Append(")");

            return sb.ToString();
        }

        public string VisitGroupingExpr(GroupingExpr expr)
        {
            return Parenthesize("group", expr.Expression);
        }

        public string VisitLiteralExpr(LiteralExpr expr)
        {
            return expr?.Value.ToString() ?? "nil";
        }

        public string VisitLogicalExpr(LogicalExpr expr)
        {
            return Parenthesize(expr.Oper.Lexeme, expr.Left, expr.Right);
        }

        public string VisitUnaryExpr(UnaryExpr expr)
        {
            return Parenthesize(expr.Oper.Lexeme, expr.Right);
        }

        public string VisitVarExpr(VarExpr expr)
        {
            return expr.Name.Lexeme;
        }

        private string Parenthesize(string name, params AstNode[] exprs)
        {
            var sb = new StringBuilder();

            sb.Append($"({name}");
            foreach (var expr in exprs)
            {
                sb.Append($" {expr.Accept(this)}");
            }
            sb.Append(")");

            return sb.ToString();
        }

        public string VisitClassStmt(ClassStmt astNode)
        {
            throw new NotImplementedException();
        }

        public string VisitGetExpr(GetExpr astNode)
        {
            throw new NotImplementedException();
        }

        public string VisitSetExpr(SetExpr astNode)
        {
            throw new NotImplementedException();
        }

        public string VisitThisExpr(ThisExpr expr)
        {
            return "this";
        }
    }
}
