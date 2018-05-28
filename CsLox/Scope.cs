using System;
using System.Collections.Generic;
using System.Text;

namespace CsLox
{
    public class Scope
    {
        private readonly Scope _parent;
        private readonly Dictionary<string, object> values = new Dictionary<string, object>();

        public Scope()
        {
            _parent = null;
        }

        public Scope(Scope parent)
        {
            _parent = parent;
        }

        public void Define (Token name, object value)
        {
            if (values.ContainsKey(name.Lexeme))
            {
                throw new LoxRuntimeException(name, $"Redefinition of '{name.Lexeme}'.");
            }

            values.Add(name.Lexeme, value);
        }

        public void Define (ICallable callable)
        {
            if (values.ContainsKey(callable.Name))
            {
                throw new LoxRuntimeException(null, $"Redefinition of '{callable.Name}'.");
            }

            values.Add(callable.Name, callable);
        }

        public void Assign(Token name, object value)
        {
            if (!values.ContainsKey(name.Lexeme))
            {
                if (_parent == null)
                {
                    throw new LoxRuntimeException(name, $"Undefined variable '{name.Lexeme}'.");
                }
                _parent.Assign(name, value);
            }

            values[name.Lexeme] = value;
        }

        public object Get(Token name)
        {
            if (!values.TryGetValue(name.Lexeme, out object value))
            {
                if (_parent == null)
                {
                    throw new LoxRuntimeException(name, $"Undefined variable '{name.Lexeme}'.");
                }

                value = _parent.Get(name);
            }

            return value;
        }
    }
}
