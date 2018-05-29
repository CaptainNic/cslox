using System;
using System.Collections.Generic;
using System.Text;

namespace CsLox
{
    public class Scope
    {
        private readonly Scope _parent;
        private readonly Dictionary<string, object> _values = new Dictionary<string, object>();

        protected Scope Parent => _parent;

        public Scope()
        {
            _parent = null;
        }

        public Scope(Scope parent)
        {
            _parent = parent;
        }

        public void Define (string name, object value)
        {
            if (_values.ContainsKey(name))
            {
                throw new LoxRuntimeException(null, $"Redefinition of '{name}'.");
            }

            _values.Add(name, value);
        }

        public void Define (ICallable callable)
        {
            if (_values.ContainsKey(callable.Name))
            {
                throw new LoxRuntimeException(null, $"Redefinition of '{callable.Name}'.");
            }

            _values.Add(callable.Name, callable);
        }

        public void Assign(Token name, object value)
        {
            if (!_values.ContainsKey(name.Lexeme))
            {
                if (_parent == null)
                {
                    throw new LoxRuntimeException(name, $"Undefined variable '{name.Lexeme}'.");
                }
                _parent.Assign(name, value);
            }

            _values[name.Lexeme] = value;
        }

        public void AssignAt(int distance, Token name, object value)
        {
            Ancestor(distance).AssignLocal(name, value);
        }

        protected void AssignLocal(Token name, object value)
        {
            if (!_values.ContainsKey(name.Lexeme))
            {
                throw new LoxRuntimeException(name, $"Undefined local variable '{name.Lexeme}'.");
            }

            _values[name.Lexeme] = value;
        }

        public object Get(Token name)
        {
            if (!_values.TryGetValue(name.Lexeme, out object value))
            {
                if (_parent == null)
                {
                    throw new LoxRuntimeException(name, $"Undefined variable '{name.Lexeme}'.");
                }

                value = _parent.Get(name);
            }

            return value;
        }

        public object GetAt(int distance, string name)
        {
            return Ancestor(distance).GetLocal(name);
        }

        private object GetLocal(string name)
        {
            if (!_values.TryGetValue(name, out object value))
            {
                throw new LoxRuntimeException(null, $"Undefined local variable '{name}'.");
            }

            return value;
        }

        private Scope Ancestor(int distance)
        {
            var scope = this;
            for (var i = 0; i < distance; ++i)
            {
                scope = scope.Parent;
            }

            return scope;
        }
    }
}
