using System;
using System.Collections.Generic;
using System.Text;

namespace CsLox.BuiltIns
{
    class Clock : ICallable
    {
        string ICallable.Name => "clock";

        int ICallable.Arity => 0;

        object ICallable.Call(Interpreter interpreter, List<object> args)
        {
            return DateTime.Now.Millisecond;
        }
    }
}
