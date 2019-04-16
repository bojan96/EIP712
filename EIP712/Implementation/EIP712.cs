using EIP712.Attributes;
using EIP712.Exceptions;
using EIP712.Utilities;
using Nethereum.ABI.Encoders;
using Nethereum.Signer;
using Nethereum.Util;
using System;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;

namespace EIP712
{
    public static class EIP712
    {

        private static readonly byte[] _eip191Header = new byte[] { 0x19, 0x1 };
        private static readonly Sha3Keccack _keccak = Sha3Keccack.Current;

        #region PublicApi
        /// <summary>
        /// Encodes structured data according to EIP-712 specification
        /// </summary>
        /// <typeparam name="T">Structured data datatype</typeparam>
        /// <param name="structure">Structured data to encode</param>
        /// <param name="domain">EIP-712 domain</param>
        /// <returns>Encoded data</returns>
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

        /// <summary>
        /// Hash structured data 
        /// </summary>
        /// <typeparam name="T">Structured data datatype</typeparam>
        /// <param name="structure">Structured data to hash</param>
        /// <param name="domain">EIP-712 domain</param>
        /// <returns>Keccak256 of encoded data</returns>
        public static byte[] Hash<T>(T structure, EIP712Domain domain) where T : class
        {
            if (structure == null)
                throw new ArgumentNullException(nameof(structure));
            if (domain == null)
                throw new ArgumentNullException(nameof(domain));

            return _keccak.CalculateHash(Encode(structure, domain));
        }

        /// <summary>
        /// Sign structured data
        /// </summary>
        /// <typeparam name="T">Structured data datatype</typeparam>
        /// <param name="structure">Structured data to hash</param>
        /// <param name="domain">EIP-712 domain</param>
        /// <param name="privateKey">Ethereum private key</param>
        /// <returns><see cref="EthereumSignature"/></returns>
        public static EthereumSignature Sign<T>(T structure, EIP712Domain domain, string privateKey) where T : class
        {
            if (structure == null)
                throw new ArgumentNullException(nameof(structure));
            if (domain == null)
                throw new ArgumentNullException(nameof(domain));
            if (privateKey == null)
                throw new ArgumentNullException(nameof(privateKey));

            EthECDSASignature sig = new MessageSigner().SignAndCalculateV(Hash(structure, domain), privateKey);

            // Returning custom type since we do not want force lib users to install additional package  (i.e. Nethereum)
            return new EthereumSignature(sig.R, sig.S, sig.V);
        }
        #endregion

        #region Implementation

        /// <summary>
        /// Implements hashStruct from EIP712
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="structure"></param>
        /// <returns></returns>
        private static byte[] HashStruct<T>(T structure) where T : class
        {
            Type structType = structure.GetType();

            // Get all properties on which StructTypeAttribute is applied and order them by "Order" property
            Tuple<PropertyInfo, MemberAttribute>[] props = structType.GetProperties().
                Where(prop => prop.CustomAttributes.Any(attr => attr.AttributeType == typeof(MemberAttribute))).
                Select(prop => Tuple.Create(prop, prop.GetCustomAttribute<MemberAttribute>())).
                OrderBy(propAttrPair => propAttrPair.Item2.Order).ToArray();

            byte[] typeHash = CalculateTypeHash(structure, props);
            byte[] encodedData = EncodeData(structure, props);
            byte[] merged = ByteUtil.Merge(typeHash, encodedData);
            return _keccak.CalculateHash(merged);
        }

        private static byte[] CalculateTypeHash<T>(T structure,
            Tuple<PropertyInfo, MemberAttribute>[] props)
        {

            Type structType = structure.GetType();
            StructNameAttribute nameAttr = structType.GetCustomAttribute<StructNameAttribute>();
            string encodedType = $"{(nameAttr == null ? structType.Name : nameAttr.Name)}(";

            string nameTypeConcatenated = props.Aggregate(string.Empty, (accumulated, prop) =>
            {

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

        private static byte[] EncodeData<T>(T structure, Tuple<PropertyInfo, MemberAttribute>[] props)
        {
            byte[] result = new byte[0];
            byte[] part = null;
            object val = null;

            foreach (var prop in props)
            {

                // TODO: Support more types
                // TODO: Implement proper error handling

                val = prop.Item1.GetValue(structure);
                string abiType = prop.Item2.AbiType;
                PropertyInfo propInfo = prop.Item1;

                // Do not encode properties with null values
                if (val == null)
                    continue;

                switch (abiType)
                {
                    case "address":
                        if (!(val is string))
                            throw new MemberTypeException(propInfo.Name, abiType, propInfo.PropertyType);
                        part = new AddressTypeEncoder().Encode(val);
                        break;

                    case "uint256":
                        if (!(Util.IsNumber(val)))
                            throw new MemberTypeException(propInfo.Name, abiType, propInfo.PropertyType);
                        part = new IntTypeEncoder(false, 32).Encode(val);
                        break;

                    case "bytes":
                        part = _keccak.CalculateHash((byte[])val);
                        break;

                    case "string":
                        part = _keccak.CalculateHash(Encoding.UTF8.GetBytes((string)val));
                        break;

                    case "bytes32":
                        part = new Bytes32TypeEncoder().Encode(val);
                        break;

                    default:
                        throw new NotSupportedException(
                            $"Can not encode property {prop.Item1.Name}, type encoding for {prop.Item2.AbiType} not supported");
                }

                Debug.Assert(part.Length == 32);

                result = ByteUtil.Merge(result, part);
            }
            return result;
        }

        #endregion

    }
}
