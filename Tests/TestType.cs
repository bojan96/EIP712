using EIP712.Attributes;
using System.Numerics;

namespace Tests
{
    class TestType
    {
        [Member("string", 1)]
        public string StringType { get; set; }

        [Member("address", 2)]
        public string AddressType { get; set; }

        [Member("uint256", 3)]
        public BigInteger? IntegerType { get; set; }

    }
}
