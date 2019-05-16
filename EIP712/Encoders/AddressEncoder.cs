using EIP712.Exceptions;
using Nethereum.ABI.Encoders;
using System;

namespace EIP712.Encoders
{
    internal class AddressEncoder : IEncoder
    {
        private static readonly AddressTypeEncoder _addressEncoder 
            = new AddressTypeEncoder();

        public byte[] Encode(object val)
        {
            if (!(val is string))
                throw new ArgumentException("Argument not a string", nameof(val));

            return _addressEncoder.Encode(val);
        }
    }
}
