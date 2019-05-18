using System;

namespace EIP712.Exceptions
{
    public class MemberTypeException : Eip712Exception
    {

        internal MemberTypeException(string propertyName, string abiType, Type propertyType)
            : base($"Property and abi types are not compatible for property {propertyName}")
        {
            PropertyName = propertyName;
            AbiType = abiType;
            PropertyType = propertyType;
        }

        public string PropertyName { get; }
        public string AbiType { get; }
        public Type PropertyType { get; }
    }
}
