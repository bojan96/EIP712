using EIP712.Attributes;
using System.Numerics;

namespace Tests
{
    class TupleType
    {
        [Member("string", 1)]
        public string StringType { get; set; }
    }
}
