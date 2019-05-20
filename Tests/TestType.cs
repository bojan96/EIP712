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

        [Member("bool", 4)]
        public bool? BoolType { get; set; }

        [Member("bytes", 5)]
        public byte[] BytesType { get; set; }

        [Member("bytes16", 6)]
        public byte[] Bytes16Type { get; set; }

        [Member("Test", 7)]
        public TupleType TupleType { get; set; }

    }
}
