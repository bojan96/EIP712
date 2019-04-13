using EIP712.Attributes;
using System.Numerics;

namespace EIP712
{
    public class EIP712Domain
    {
        // TODO: Add docs
        [StructType("string", 1)]
        public string Name { get; set; }

        [StructType("string", 2)]
        public string Version { get; set; }

        [StructType("uint256", 3)]
        public BigInteger? ChainId { get; set; }

        [StructType("address", 4)]
        public string VerifyingContract { get; set; }

        [StructType("bytes32", 5)]
        public string Salt { get; set; }
    }
}
