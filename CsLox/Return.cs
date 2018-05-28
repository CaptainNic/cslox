using System;
using System.Runtime.Serialization;

namespace CsLox
{
    [Serializable]
    internal class Return : Exception
    {
        public readonly object Value;

        public Return()
        {
        }

        public Return(object value)
        {
            Value = value;
        }

        public Return(string message) : base(message)
        {
        }

        public Return(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected Return(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}