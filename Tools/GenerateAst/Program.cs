using System;
using System.Collections.Generic;
using System.IO;

namespace CsLox.Tools
{
    public class GenerateAst
    {
        static void Main(string[] args)
        {
            if (args.Length != 1)
            {
                throw new ArgumentException("Usage: GenerateAst <directory>");
            }

            var outDir = args[0];
        }

        private static void DefineAst(string outputDir, string baseName, List<string> types)
        {
            var path = Path.Combine(outputDir, $"{baseName}.cs");

            using(var fs = File.OpenWrite(path))
            using(var sw = new StreamWriter(fs))
            {
                sw.WriteLine( "namespace Cslox");
                sw.WriteLine( "{");
                sw.WriteLine($"    abstract class {baseName}");
                sw.WriteLine( "    {");
                sw.WriteLine( "    }");

                foreach (var type in types)
                {
                    var className = type.Split(":")[0].Trim();
                    var fields = type.Split(":")[1].Trim();
                    DefineType(sw, baseName, className, fields);
                }

                sw.WriteLine( "}");
            }
        }

        private static void DefineType(StreamWriter sw, string baseName, string className, string fields)
        {
            // Start class
            sw.WriteLine();
            sw.WriteLine($"    class {className}{baseName} : {baseName}");
            sw.WriteLine( "    {");

            // Fields
            foreach (var field in fields.Split(", "))
            {
                sw.WriteLine($"        readonly {field};");
            }

            // Constructor
            sw.WriteLine($"        {className}({fields})");
            sw.WriteLine( "        {");
            foreach (var field in fields.Split(", "))
            {
                var name = field.Split(" ")[1];
                sw.WriteLine($"            this.{name} = {name};");
            }
            sw.WriteLine( "        }");

            // End class
            sw.WriteLine( "    }");
        }
    }
}