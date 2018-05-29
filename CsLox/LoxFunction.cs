using System;
using System.Collections.Generic;
using System.Text;

namespace CsLox
{
    class LoxFunction : ICallable
    {
        private readonly FunctionStmt _declaration;
        private readonly Scope _closure;
        private readonly Boolean _isInitializer;

        public LoxFunction(FunctionStmt declaration, Scope closure, bool isInitializer)
        {
            _declaration = declaration;
            _closure = closure;
            _isInitializer = isInitializer;
        }

        public LoxFunction Bind(LoxInstance instance)
        {
            var scope = new Scope(_closure);
            scope.Define("this", instance);

            return new LoxFunction(_declaration, scope, _isInitializer);
        }

        public string Name => $"<fn {_declaration.Name.Lexeme}>";

        public int Arity => _declaration.Parameters.Count;

        public object Call(Interpreter interpreter, List<object> args)
        {
            var scope = new Scope(_closure);
            for (var i = 0; i < args.Count; ++i)
            {
                scope.Define(_declaration.Parameters[i].Lexeme, args[i]);
            }

            // Return is handled as an exception to take advantage of
            // the stack unwinding to take us back to the callsite.
            try
            {
                interpreter.ExecuteBlock(_declaration.Body, scope);
            }
            catch (Return ret)
            {
                return (_isInitializer)
                    ? _closure.GetAt(0, "this")
                    : ret.Value;
            }

            return (_isInitializer)
                ? _closure.GetAt(0, "this")
                : null;
        }
    }
}
