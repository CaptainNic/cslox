using System.Collections.Generic;

namespace CsLox
{
    internal class LoxClass : ICallable
    {
        private readonly string _name;
        private readonly LoxClass _superClass;
        private readonly Dictionary<string, LoxFunction> _methods;

        public LoxClass(string name, LoxClass superClass, Dictionary<string, LoxFunction> methods)
        {
            _name = name;
            _superClass = superClass;
            _methods = methods;
        }

        public LoxFunction FindMethod(LoxInstance instance, string name)
        {
            if (_methods.TryGetValue(name, out LoxFunction method))
            {
                return method.Bind(instance);
            }

            return _superClass?.FindMethod(instance, name);
        }

        public int Arity
        {
            get
            {
                if (_methods.TryGetValue("init", out LoxFunction initializer))
                {
                    return initializer.Arity;
                }

                return 0;
            }
        }

        public string Name => _name;

        public object Call(Interpreter interpreter, List<object> args)
        {
            var instance = new LoxInstance(this);
            if (_methods.TryGetValue("init", out LoxFunction initializer))
            {
                initializer.Bind(instance).Call(interpreter, args);
            }

            return instance;
        }

        public override string ToString()
        {
            return _name;
        }
    }
}