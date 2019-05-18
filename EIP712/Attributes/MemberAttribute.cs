using EIP712.Utilities;
using System;
using System.Linq;

namespace EIP712.Attributes
{
    public class MemberAttribute : Attribute
    {

        public string AbiType { get; }
        public int Order { get; }

        public MemberAttribute(string abiType, int order)
        {
            AbiType = abiType;
            Order = order;
        }
    }
}
