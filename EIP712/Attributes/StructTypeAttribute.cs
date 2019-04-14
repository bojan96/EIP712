using System;
using System.Linq;

namespace EIP712.Attributes
{
    public class StructTypeAttribute : Attribute
    {

        public string AbiType { get; }
        public int Order { get; }

        public StructTypeAttribute(string abiType, int order)
        {
            if (abiType == null)
                throw new ArgumentNullException(nameof(abiType));
            else if (!Utilities.Utilities.AllowedTypes.Contains(abiType))
                throw new ArgumentException("Given type not supported or valid", nameof(abiType));

            AbiType = abiType;
            Order = order;
        }
    }
}
