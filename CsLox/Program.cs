using System;
using System.IO;

namespace CsLox
{
    class Lox
    {
        private static bool _hadError = false;

        static void Main(string[] args)
        {
            if (args.Length == 0)
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

            foreach (var token in tokens)
            {
                Console.WriteLine(token);
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
