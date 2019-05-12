using Nethereum.Util;
using System.Diagnostics;
using System.Numerics;

namespace EIP712.Utilities
{
    internal static class Util
    {

        /// <summary>
        /// Supported ABI types
        /// </summary>
        public static readonly string[] AllowedTypes =
            new string[]
            {
                // TODO: Add support for more types
                "address",
                "uint256",
                "bytes",
                "bytes32",
                "string"
            };


        public static bool IsNumber(object val)
            => val is int || val is uint || val is short 
            || val is ushort || val is long || val is ulong 
            || val is byte || val is sbyte || val is BigInteger;

        /// <summary>
        /// Pads byte array with zeros to specified length (left padding)
        /// </summary>
        /// <param name="bytes">byte array to pad</param>
        /// <param name="length">pad size</param>
        /// <returns></returns>
        public static byte[] PadBytes(byte[] bytes, int length)
        {
            Debug.Assert(bytes != null);
            Debug.Assert(length > 0);

            return bytes.Length >= length ? bytes : ByteUtil.Merge(new byte[length - bytes.Length], bytes);
        }
        
    }
}
