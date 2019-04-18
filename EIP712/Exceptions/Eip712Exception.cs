using System;

namespace EIP712.Exceptions
{
    public class Eip712Exception : Exception
    {
        public Eip712Exception(string message, Exception inner = null) 
            : base(message, inner) { }
    }
}
