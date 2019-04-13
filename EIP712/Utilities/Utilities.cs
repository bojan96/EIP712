using System;
using System.Collections.Generic;
using System.Text;

namespace EIP712.Utilities
{
    internal static class Utilities
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
    }
}
