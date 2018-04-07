using System;
using System.Collections.Generic;
using System.IO;

namespace CsLox
{
    class Lox
    {
        private static bool _hadError = false;
        private static string _scriptPath = "";

        // Debug Options
        private static bool _dbgLexer = false;
        private static bool _dbgAst = false;

        static void Main(string[] args)
        {
            ParseOptions(args);
            if (string.IsNullOrWhiteSpace(_scriptPath))
            {
                RunPrompt();
            }
            else 
            {
                RunScript(args[0]);
            }

            if (_hadError)
            {
                Environment.Exit(-1);
            }
        }

        private static void ParseOptions(string[] args)
        {
            List<string> opts = new List<string>(args);

            void PerformIfOptFound(string opt, Action<int> setter)
            {
                var index = opts.FindIndex((str) => str.Equals(opt));
                if (index >= 0)
                {
                    setter(index);
                }
            }

            PerformIfOptFound("--dbg-lexer", (idx) => _dbgLexer = true);
            PerformIfOptFound(  "--dbg-ast", (idx) => _dbgAst = true);
            PerformIfOptFound(   "--script", (idx) => _scriptPath = opts[idx]);
        }



        private static void RunScript(string scriptPath)
        {
            string code = File.ReadAllText(scriptPath);
            Eval(code);
        }

        private static void RunPrompt()
        {
            Console.WriteLine("CsLox: Interactive Mode");
            Console.WriteLine("=======================");

            for (;;) {
                Console.Write("> ");
                Eval(Console.ReadLine());

                // Reset error state so user can keep making mistakes.
                _hadError = false;
            }
        }

        private static void Eval(string code)
        {
            Lexer lexer = new Lexer(code);
            var tokens = lexer.ScanTokens();

            if (_dbgLexer)
            {
                foreach (var token in tokens)
                {
                    Console.WriteLine(token);
                }
            }

            if (_dbgAst)
            {
                var astPrinter = new AstPrinter();
                Expr expr = new BinaryExpr(
                    new UnaryExpr(
                        new Token(TokenType.MINUS, "-", null, 1),
                        new LiteralExpr(123)),
                    new Token(TokenType.STAR, "*", null, 1),
                    new GroupingExpr(
                        new LiteralExpr(45.67)
                    )        
                );
                Console.WriteLine(astPrinter.Print(expr));
            }
        }

        public static void Error(int line, string message)
        {
            Report(line, "", message);
        }

        private static void Report(int line, string where, string message)
        {
            Console.Error.WriteLine($"[line {line}] Error {where}: {message}");
            _hadError = true;
        }
    }
}
