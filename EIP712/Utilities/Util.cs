using Nethereum.Util;
using System.Diagnostics;
using System.Linq;
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
                "bytes",
                "string",
                "tuple",
                "bool"
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

        public static bool IsValidAbiType(string abiType)
        {
            return AllowedTypes.Contains(abiType) 
                || IsValidBytesType(abiType, out int sizeBytes) 
                || IsValidIntegerAbiType(abiType, out int sizeInts);
        }

        /// <summary>
        /// Determines whether specified ABI type is valid integer type
        /// </summary>
        /// <param name="intAbiType">ABI type</param>
        /// <returns></returns>
        public static bool IsValidIntegerAbiType(string intAbiType, out int size)
        {
            size = 0;

            if (intAbiType.StartsWith("uint") || intAbiType.StartsWith("int"))
            {
                string sizePart = intAbiType.Substring(intAbiType.StartsWith("uint") ? 4 : 3);
                return sizePart == string.Empty ||
                    int.TryParse(sizePart, out size) &&
                    size % 8 == 0 && size >= 8 && size <= 256;
            }
            else
                return false;
        }

        /// <summary>
        /// Determines whether specified abiType is valid bytesN type
        /// </summary>
        /// <param name="bytesAbiType"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static bool IsValidBytesType(string bytesAbiType, out int size)
        {
            size = 0;

            return bytesAbiType.StartsWith("bytes") &&
                int.TryParse(bytesAbiType.Substring(5), out size)
                && size > 0 && size <= 32;
        }
        
    }
}
