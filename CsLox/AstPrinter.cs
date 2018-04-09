using System;
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
            throw new NotImplementedException();
        }

        public string VisitExpressionStmt(ExpressionStmt stmt)
        {
            throw new NotImplementedException();
        }

        public string VisitIfStmt(IfStmt stmt)
        {
            throw new NotImplementedException();
        }

        public string VisitPrintStmt(PrintStmt stmt)
        {
            throw new NotImplementedException();
        }

        public string VisitVarDeclStmt(VarDeclStmt stmt)
        {
            throw new NotImplementedException();
        }

        public string VisitAssignExpr(AssignExpr expr)
        {
            throw new NotImplementedException();
        }

        public string VisitBinaryExpr(BinaryExpr expr)
        {
            return Parenthesize(expr.Oper.Lexeme, expr.Left, expr.Right);
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
            throw new NotImplementedException();
        }

        public string VisitUnaryExpr(UnaryExpr expr)
        {
            return Parenthesize(expr.Oper.Lexeme, expr.Right);
        }

        public string VisitVarExpr(VarExpr expr)
        {
            throw new NotImplementedException();
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
    }
}
