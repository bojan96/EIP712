using Nethereum.Util;
using System;
using System.Text;

namespace EIP712.Encoders
{
    internal class StringEncoder : IEncoder
    {
        public byte[] Encode(object val)
        {
            if (!(val is string str))
                throw new ArgumentException("Argument not a string", nameof(val));

            return Sha3Keccack.Current.CalculateHash(Encoding.UTF8.GetBytes(str));
        }
    }
}
