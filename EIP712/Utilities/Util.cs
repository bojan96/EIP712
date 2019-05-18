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

        /// <summary>
        /// Determines whether specified type is valid ABI type
        /// </summary>
        /// <param name="abiType">ABI type</param>
        /// <returns>true if valid, false otherwise</returns>
        public static bool IsValidAbiType(string abiType)
        {
            Debug.Assert(abiType != null);
            return AllowedTypes.Contains(abiType) 
                || IsValidBytesType(abiType, out int sizeBytes) 
                || IsValidIntegerAbiType(abiType, out int sizeInts);
        }

        /// <summary>
        /// Determines whether specified ABI type is valid integer type
        /// </summary>
        /// <param name="intAbiType">ABI type</param>
        /// <returns>true if specified type is valid integer type with size set to size of 
        /// integer type, otherwise false with size equal to zero</returns>
        public static bool IsValidIntegerAbiType(string intAbiType, out int size)
        {
            Debug.Assert(intAbiType != null);

            size = 0;
            bool signed = false;
            bool valid = false;

            if (intAbiType.StartsWith("uint") || (signed = intAbiType.StartsWith("int")))
            {
                string sizePart = intAbiType.Substring(signed ? 3 : 4);
                if (sizePart == string.Empty)
                {
                    size = 256;
                    valid = true;
                }
                else
                {
                    int integerSize;

                    valid = int.TryParse(sizePart, out integerSize) 
                        && integerSize % 8 == 0 && integerSize >= 8 
                        && integerSize <= 256;
                    size = valid ? integerSize : 0;
                }
            }

            return valid;
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
