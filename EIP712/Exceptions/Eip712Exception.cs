using System;

namespace EIP712.Exceptions
{
    public abstract class Eip712Exception : Exception
    {
        public Eip712Exception(string message, Exception inner = null) 
            : base(message, inner) { }
    }
}
