using EIP712.Attributes;
using System.Numerics;

namespace EIP712
{
    public class EIP712Domain
    {
        // TODO: Add docs
        [Member("string", 1)]
        public string Name { get; set; }

        [Member("string", 2)]
        public string Version { get; set; }

        [Member("uint256", 3)]
        public BigInteger? ChainId { get; set; }

        [Member("address", 4)]
        public string VerifyingContract { get; set; }

        [Member("bytes32", 5)]
        public byte[] Salt { get; set; }
    }
}
