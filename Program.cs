using System;
using System.IO;

namespace cslox
{
    class Program
    {
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
            }
        }

        private static void Eval(string code)
        {
            if (!string.IsNullOrWhiteSpace(code))
            {
                Console.WriteLine("Lox evaluation not yet implemented.");
            }
        }
    }
}
