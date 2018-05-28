using System;
using System.Collections.Generic;
using System.IO;

namespace CsLox
{
    class Lox
    {
        private static bool _hadError = false;
        private static bool _hadRuntimeError = false;
        private static string _scriptPath = "";

        // Debug Options
        private static bool _dbgLexer = false;
        private static bool _dbgAst = false;

        private static Interpreter _interpreter = new Interpreter();

        static void Main(string[] args)
        {
            ParseOptions(args);
            if (string.IsNullOrWhiteSpace(_scriptPath))
            {
                RunPrompt();
            }
            else 
            {
                RunScript(_scriptPath);
            }

            if (_hadError)
            {
                Environment.Exit(65);
            }
            if (_hadRuntimeError)
            {
                Environment.Exit(70);
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
            PerformIfOptFound(   "--script", (idx) => _scriptPath = opts[idx + 1]);
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

            Parser parser = new Parser(tokens);
            var statements = parser.Parse();

            if (_dbgAst)
            {
                AstPrinter astPrinter = new AstPrinter();
                foreach (var statement in statements)
                {
                    Console.WriteLine(astPrinter.Print(statement));
                }
            }

            _interpreter.Interpret(statements);
        }

        public static void Error(int line, string message)
        {
            Report(line, "", message);
        }

        public static void Error(Token token, string message)
        {
            if (token.Type == TokenType.EOF)
            {
                Report(token.Line, " at end", message);
            }
            else
            {
                Report(token.Line, $" at '{token.Lexeme}'", message);
            }
        }

        public static void RuntimeError(LoxRuntimeException ex)
        {
            Console.Error.WriteLine($"[Line {ex.Token.Line}] {ex.Message}");
            _hadRuntimeError = true;
        }

        private static void Report(int line, string where, string message)
        {
            Console.Error.WriteLine($"[line {line}] Error {where}: {message}");
            _hadError = true;
        }
    }
}
