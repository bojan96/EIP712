using EIP712.Attributes;

namespace Tests
{
    class TestType
    {
        [Member("string", 1)]
        public string StringType { get; set; }

        [Member("address", 2)]
        public string AddressType { get; set; }
    }
}
