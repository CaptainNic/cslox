using System.Collections.Generic;

namespace CsLox
{
    internal class LoxInstance
    {
        private LoxClass _klass;
        private readonly Dictionary<string, object> _fields = new Dictionary<string, object>();

        public LoxInstance(LoxClass klass)
        {
            _klass = klass;
        }

        public object Get(Token name)
        {
            if (_fields.TryGetValue(name.Lexeme, out object value))
            {
                return value;
            }

            var method = _klass.FindMethod(this, name.Lexeme);
            if (method != null)
            {
                return method;
            }

            throw new LoxRuntimeException(name, $"Undefined property '{name.Lexeme}'.");
        }

        public void Set(Token name, object value)
        {
            _fields.Add(name.Lexeme, value);
        }

        public override string ToString()
        {
            return $"{_klass.Name} instance";
        }
    }
}