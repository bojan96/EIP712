using EIP712.Attributes;
using EIP712.Exceptions;
using EIP712.Utilities;
using Nethereum.ABI.Encoders;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Signer;
using Nethereum.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;

namespace EIP712
{
    public static class EIP712Service
    {

        private static readonly byte[] _eip191Header = new byte[] { 0x19, 0x1 };
        private static readonly Sha3Keccack _keccak = Sha3Keccack.Current;
        private static readonly MessageSigner _signer = new MessageSigner();

        #region PublicApi
        /// <summary>
        /// Encodes structured data according to EIP-712 specification
        /// </summary>
        /// <typeparam name="T">Structured data datatype</typeparam>
        /// <param name="structure">Structured data to encode</param>
        /// <param name="domain">EIP-712 domain</param>
        /// <returns>Encoded data</returns>
        /// <exception cref="ArgumentNullException"><paramref name="structure"/> or <paramref name="domain"/> is equal to <c>null</c></exception>
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
        /// <exception cref="ArgumentNullException"><paramref name="domain"/> or <paramref name="structure"/> 
        /// is equal to <c>null</c></exception>
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
        /// <param name="structure">Structured data to sign</param>
        /// <param name="domain">EIP-712 domain</param>
        /// <param name="privateKey">Ethereum private key</param>
        /// <returns><see cref="EthereumSignature"/></returns>
        /// <exception cref="ArgumentNullException">Any of the arguments is equal to <c>null</c></exception>
        public static EthereumSignature Sign<T>(T structure, EIP712Domain domain, string privateKey) where T : class
        {
            if (structure == null)
                throw new ArgumentNullException(nameof(structure));
            if (domain == null)
                throw new ArgumentNullException(nameof(domain));
            if (privateKey == null)
                throw new ArgumentNullException(nameof(privateKey));

            EthECDSASignature sig = _signer.SignAndCalculateV(Hash(structure, domain), privateKey);

            // Returning custom type since we do not want force lib users to install additional package  (i.e. Nethereum)
            return new EthereumSignature(Util.PadBytes(sig.R, 32), Util.PadBytes(sig.S, 32), sig.V);
        }


        /// <summary>
        /// Verify signature 
        /// </summary>
        /// <typeparam name="T">Structured data datatype</typeparam>
        /// <param name="structure">Structured data for which to verify signature</param>
        /// <param name="domain">EIP-712 domain</param>
        /// <param name="signerAddress">Ethereum address</param>
        /// <param name="signature">Signature of the structured data</param>
        /// <returns><c>true</c> if signature is valid <c>false</c> otherwise</returns>
        /// <exception cref="ArgumentNullException">Any of the arguments is equal to <c>null</c></exception>
        /// <exception cref="ArgumentException">Invalid Ethereum address</exception>
        public static bool VerifySignature<T>(T structure, EIP712Domain domain, string signerAddress, byte[] signature) where T : class
        {
            structure = structure ?? throw new ArgumentNullException(nameof(structure));
            domain = domain ?? throw new ArgumentNullException(nameof(domain));
            signerAddress = signerAddress ?? throw new ArgumentNullException(nameof(signerAddress));
            signature = signature ?? throw new ArgumentNullException(nameof(signature));

            if (! AddressUtil.Current.IsValidEthereumAddressHexFormat(signerAddress))
                throw new ArgumentException("Invalid ethereum address", nameof(signerAddress));

            string recoveredAddress = _signer.EcRecover(Hash(structure, domain), signature.ToHex(true));
            return AddressUtil.Current.AreAddressesTheSame(recoveredAddress, signerAddress);
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
            byte[] typeHash = CalculateTypeHash(structure);
            byte[] encodedData = EncodeData(structure);
            byte[] merged = ByteUtil.Merge(typeHash, encodedData);
            return _keccak.CalculateHash(merged);
        }

        /// <summary>
        /// typeHash implementation from EIP712 spec
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="structure"></param>
        /// <returns></returns>
        private static byte[] CalculateTypeHash<T>(T structure)
        {
            string encodedType = EncodeType(structure);
            Dictionary<string, string> depTypes = new Dictionary<string, string>();

            // Find all types nested in passed structure
            FindNestedTypes(structure, depTypes);

            // Order types by name (ascending), concatenate all encodings
            encodedType = depTypes.ToList()
                .OrderBy(t => t.Key)
                .Aggregate(encodedType, 
                (accumulated, t) => accumulated + t.Value);
            return _keccak.CalculateHash(Encoding.UTF8.GetBytes(encodedType));
        }

        private static byte[] EncodeData<T>(T structure)
        {
            byte[] result = new byte[0];
            byte[] part = null;

            foreach (var prop in structure.GetMemberProperties())
            {
                // TODO: Support array types

                object val = prop.Item1.GetValue(structure);
                string memberType = prop.Item2.Type;
                PropertyInfo propInfo = prop.Item1;

                if (memberType == "address")
                {
                    if (!(val is string))
                        throw new MemberTypeException(propInfo.Name, memberType, propInfo.PropertyType);
                    part = new AddressTypeEncoder().Encode(val);
                }
                else if (memberType == "bytes")
                {
                    if (!(val is byte[] value))
                        throw new MemberTypeException(propInfo.Name, memberType, propInfo.PropertyType);
                    part = _keccak.CalculateHash(value);
                }
                else if (memberType == "string")
                {
                    if (!(val is string str))
                        throw new MemberTypeException(propInfo.Name, memberType, propInfo.PropertyType);
                    part = _keccak.CalculateHash(Encoding.UTF8.GetBytes(str));
                }
                else if (memberType == "bool")
                {
                    if (!(val is bool))
                        throw new MemberTypeException(propInfo.Name, memberType, propInfo.PropertyType);
                    part = new BoolTypeEncoder().Encode(val);
                }
                else if (Util.IsValidIntegerAbiType(memberType, out int intSize, out bool signed))
                {
                    if (!Util.IsNumber(val))
                        throw new MemberTypeException(propInfo.Name, memberType, propInfo.PropertyType);
                    part = new IntTypeEncoder(signed, (uint)intSize).Encode(val);
                }
                else if (Util.IsValidBytesType(memberType, out int bytesSize))
                {
                    // TODO: Validate byteArray length
                    if (!(val is byte[] byteArray))
                        throw new MemberTypeException(propInfo.Name, memberType, propInfo.PropertyType);
                    part = new BytesElementaryTypeEncoder(bytesSize).Encode(val);
                }
                else
                    part = HashStruct(val);
                
                Debug.Assert(part.Length == 32);

                result = ByteUtil.Merge(result, part);
            }
            return result;
        }

        /// <summary>
        /// encodeType implementation from EIP712 spec
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="structure"></param>
        /// <returns></returns>
        private static string EncodeType<T>(T structure)
        {
            string encodedType = $"{structure.GetStructureName()}(";

            string nameTypeConcatenated = structure.GetMemberProperties()
                .Aggregate(string.Empty, (accumulated, prop) =>
            {
                string prefix = accumulated == string.Empty ? string.Empty : ",";
                // TODO: Make property name encoding configurable

                string memberType = prop.Item2.Type;
                PropertyInfo propInfo = prop.Item1;
                string nameType = $"{prefix}{memberType} " +
                $"{propInfo.Name.ToCamelCase()}";

                return accumulated + nameType;
            });

            encodedType += $"{nameTypeConcatenated})";

            return encodedType;
        }

        /// <summary>
        /// Gets all structures nested in passed structure
        /// </summary>
        /// <typeparam name="T">Structured data datatype</typeparam>
        /// <param name="structure">Structure</param>
        /// <returns></returns>
        private static void FindNestedTypes<T>(T structure, 
            Dictionary<string, string> depTypes = null)
        {
            Tuple<PropertyInfo, MemberAttribute>[] props = structure.GetMemberProperties();
            IEnumerable<Tuple<PropertyInfo,MemberAttribute>> nestedTypesProps 
                // Properties which member type is not any of the valid 
                // ABI types is considered custom nested type
                = props.Where(prop => !Util.IsValidAbiType(prop.Item2.Type));

            foreach (var prop in nestedTypesProps)
            {
                object val = prop.Item1.GetValue(structure);
                string typeName = val.GetStructureName();

                if (!depTypes.ContainsKey(typeName))
                    depTypes[typeName] = EncodeType(val);

                // Find nested types in this structure nested type
                FindNestedTypes(val, depTypes);
            }
        }

        #endregion

    }
}
