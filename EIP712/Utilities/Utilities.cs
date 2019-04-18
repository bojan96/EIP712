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
        
    }
}
