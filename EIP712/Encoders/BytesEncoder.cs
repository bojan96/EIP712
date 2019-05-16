using Nethereum.Util;
using System;

namespace EIP712.Encoders
{
    internal class BytesEncoder : IEncoder
    {
        public byte[] Encode(object val)
        {
            if (!(val is byte[] value))
                throw new ArgumentException("Argument not a array of bytes", nameof(val));
            return Sha3Keccack.Current.CalculateHash(value);
        }
    }
}
