using EIP712.Attributes;

namespace Tests
{
    class NestedType
    {
        [Member("string", 1)]
        public string StringType { get; set; }
    }
}
