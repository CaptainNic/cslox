using System.Collections.Generic;

namespace CsLox
{
    public interface ICallable
    {
        string Name { get; }

        int Arity { get; }

        object Call(Interpreter interpreter, List<object> args);
    }
}