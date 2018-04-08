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

        public string VisitExpressionStmt(ExpressionStmt stmt)
        {
            throw new NotImplementedException();
        }

        public string VisitPrintStmt(PrintStmt stmt)
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

        public string VisitUnaryExpr(UnaryExpr expr)
        {
            return Parenthesize(expr.Oper.Lexeme, expr.Right);
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
