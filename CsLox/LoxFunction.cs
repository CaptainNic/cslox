using System;
using System.Collections.Generic;
using System.Text;

namespace CsLox
{
    class LoxFunction : ICallable
    {
        private readonly FunctionStmt _declaration;
        private readonly Scope _closure;

        public LoxFunction(FunctionStmt declaration, Scope closure)
        {
            _declaration = declaration;
            _closure = closure;
        }

        string ICallable.Name => $"<fn {_declaration.Name.Lexeme}>";

        int ICallable.Arity => _declaration.Parameters.Count;

        object ICallable.Call(Interpreter interpreter, List<object> args)
        {
            var scope = new Scope(_closure);
            for (var i = 0; i < args.Count; ++i)
            {
                scope.Define(_declaration.Parameters[i], args[i]);
            }

            // Return is handled as an exception to take advantage of
            // the stack unwinding to take us back to the callsite.
            try
            {
                interpreter.ExecuteBlock(_declaration.Body, scope);
            }
            catch (Return ret)
            {
                return ret.Value;
            }

            return null;
        }
    }
}
