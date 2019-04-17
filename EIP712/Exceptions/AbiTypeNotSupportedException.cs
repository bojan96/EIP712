using System;

namespace EIP712.Exceptions
{
    public class AbiTypeNotSupportedException : Eip712Exception
    {
        public AbiTypeNotSupportedException(string abiType, string propertyName) 
            : base($"Encodig of abi type {abiType} not supported for property {propertyName}") { }

        public string AbiType { get; }
    }
}
