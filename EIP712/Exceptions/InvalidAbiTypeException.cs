namespace EIP712.Exceptions
{
    public class InvalidAbiTypeException : Eip712Exception
    {
        internal InvalidAbiTypeException(string propertyName, string abiType) 
            : base($"Invalid ABI type \"${abiType}\" for property \"${propertyName}\"", null)
        {
            PropertyName = propertyName;
            AbiType = abiType;
        }

        public string PropertyName { get; }
        public string AbiType { get; }
    }
}
