using EIP712.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tests
{
    class TestType
    {
        [StructType("string", 1)]
        public string StringType { get; set; }

        [StructType("address", 2)]
        public string AddressType { get; set; }
    }
}
