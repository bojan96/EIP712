using EIP712.Attributes;
using EIP712.Utilities;
using Nethereum.ABI.Encoders;
using Nethereum.Util;
using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace EIP712
{
    public static class EIP712
    {

        private static readonly byte[] _eip191Header = new byte[] { 0x19, 0x1 };
        private static readonly Sha3Keccack _keccak = Sha3Keccack.Current;

        /// <summary>
        /// Encodes structured data according to EIP-712 specification
        /// </summary>
        /// <typeparam name="T">Structured data datatype</typeparam>
        /// <param name="structure">Structured data to encode</param>
        /// <param name="domain">EIP-712 domain</param>
        /// <returns></returns>
        public static byte[] Encode<T>(T structure, EIP712Domain domain) where T : class
        {
            if (domain == null)
                throw new ArgumentNullException(nameof(structure));
            if (domain == null)
                throw new ArgumentNullException(nameof(domain));

            byte[] domainSeparator = HashStruct(domain);
            byte[] hash = HashStruct(structure);
            byte[] result = ByteUtil.Merge(_eip191Header, domainSeparator, hash);
            return result;
        }


        private static byte[] HashStruct<T>(T structure) where T : class
        {
            Type structType = structure.GetType();

            // Get all properties on which StructTypeAttribute is applied
            Tuple<PropertyInfo, StructTypeAttribute>[] props = structType.GetProperties().
                Where(prop => prop.CustomAttributes.Any(attr => attr.AttributeType == typeof(StructTypeAttribute))).
                Select(prop => Tuple.Create(prop, prop.GetCustomAttribute<StructTypeAttribute>())).
                OrderBy(propAttrPair => propAttrPair.Item2.Order).ToArray();

            byte[] typeHash = CalculateTypeHash(structure, props);
            byte[] encodedData = EncodeData(structure, props);
            byte[] merged = ByteUtil.Merge(typeHash, encodedData);
            return _keccak.CalculateHash(merged);
        }

        private static byte[] CalculateTypeHash<T>(T structure,
            Tuple<PropertyInfo, StructTypeAttribute>[] props)
        {

            Type structType = structure.GetType();
            StructNameAttribute nameAttr = structType.GetCustomAttribute<StructNameAttribute>();
            string encodedType = $"{(nameAttr == null ? structType.Name : nameAttr.Name)}(";

            string nameTypeConcatenated = props.Aggregate(string.Empty, (accumulated, prop) => {

                // If values is null do not encode type
                if (prop.Item1.GetValue(structure) == null)
                    return accumulated;

                string prefix = accumulated == string.Empty ? string.Empty : ",";
                // TODO: Make property name encoding configurable
                string nameType = $"{prefix}{prop.Item2.AbiType} {prop.Item1.Name.ToCamelCase()}";

                return accumulated + nameType;
            });

            encodedType += $"{nameTypeConcatenated})";

            return _keccak.CalculateHash(Encoding.UTF8.GetBytes(encodedType));
        }

        private static byte[] EncodeData<T>(T structure, Tuple<PropertyInfo, StructTypeAttribute>[] props)
        {
            byte[] result = new byte[0];
            byte[] part = null;
            object val = null;

            foreach (var prop in props)
            {

                // TODO: Support more types
                // TODO: Implement proper error handling

                val = prop.Item1.GetValue(structure);

                // Do not encode properties with null values
                if (val == null)
                    continue;

                switch (prop.Item2.AbiType)
                {
                    case "address":
                        part = new AddressTypeEncoder().Encode(val);
                        break;

                    case "uint256":
                        part = new IntTypeEncoder(false, 32).Encode(val);
                        break;

                    case "bytes":
                        part = _keccak.CalculateHash((byte[])val);
                        break;

                    case "string":
                        byte[] temp = Encoding.UTF8.GetBytes((string)val);
                        part = _keccak.CalculateHash(temp);
                        break;

                    case "bytes32":
                        part = new Bytes32TypeEncoder().Encode(val);
                        break;

                    default:
                        throw new InvalidOperationException(
                            $"Can not encode property ${prop.Item1.Name}, type encoding for ${prop.Item2.AbiType} not supported");
                }

                Debug.Assert(part.Length == 32);

                result = ByteUtil.Merge(result, part);
            }
            return result;
        }

    }
}
