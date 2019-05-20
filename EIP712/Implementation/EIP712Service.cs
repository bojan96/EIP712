using EIP712.Attributes;
using EIP712.Exceptions;
using EIP712.Utilities;
using Nethereum.ABI.Encoders;
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
        /// <exception cref="ArgumentNullException">structure or domain is null</exception>
        /// <exception cref=""
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
            return new EthereumSignature(Util.PadBytes(sig.R, 32), Util.PadBytes(sig.S, 32), sig.V);
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

        private static byte[] CalculateTypeHash<T>(T structure)
        {
            string encodedType = EncodeType(structure);
            encodedType = FindTupleTypes(structure).Aggregate(encodedType, 
                (accumulated, t) => accumulated + t.Item2);
            return _keccak.CalculateHash(Encoding.UTF8.GetBytes(encodedType));
        }

        private static byte[] EncodeData<T>(T structure)
        {
            byte[] result = new byte[0];
            byte[] part = null;

            foreach (var prop in structure.GetMemberProperties())
            {
                // TODO: Support more types

                object val = prop.Item1.GetValue(structure);
                string abiType = prop.Item2.AbiType;
                PropertyInfo propInfo = prop.Item1;


                if (!Util.IsValidAbiType(abiType))
                    throw new InvalidAbiTypeException(propInfo.Name, abiType);

                if(abiType == "address")
                {
                    if (!(val is string))
                        throw new MemberTypeException(propInfo.Name, abiType, propInfo.PropertyType);
                    part = new AddressTypeEncoder().Encode(val);
                }
                else if(abiType == "bytes")
                {
                    if (!(val is byte[] value))
                        throw new MemberTypeException(propInfo.Name, abiType, propInfo.PropertyType);
                    part = _keccak.CalculateHash(value);
                }
                else if(abiType == "string")
                {
                    if (!(val is string str))
                        throw new MemberTypeException(propInfo.Name, abiType, propInfo.PropertyType);
                    part = _keccak.CalculateHash(Encoding.UTF8.GetBytes(str));
                }
                else if(abiType == "bool")
                {
                    if (!(val is bool))
                        throw new MemberTypeException(propInfo.Name, abiType, propInfo.PropertyType);
                    part = new BoolTypeEncoder().Encode(val);
                }
                else if(abiType == "tuple")
                    part = HashStruct(val);             
                else if(Util.IsValidIntegerAbiType(abiType, out int intSize, out bool signed))
                {
                    if (!Util.IsNumber(val))
                        throw new MemberTypeException(propInfo.Name, abiType, propInfo.PropertyType);
                    part = new IntTypeEncoder(signed, (uint)intSize).Encode(val);
                }
                else if(Util.IsValidBytesType(abiType, out int bytesSize))
                {
                    // TODO: Validate byteArray length
                    if (!(val is byte[] byteArray))
                        throw new MemberTypeException(propInfo.Name, abiType, propInfo.PropertyType);
                    part = new BytesElementaryTypeEncoder(bytesSize).Encode(val);
                }
                else
                {
                    // Unreachable
                    Debug.Assert(false);
                }

                Debug.Assert(part.Length == 32);

                result = ByteUtil.Merge(result, part);
            }
            return result;
        }

        private static string EncodeType<T>(T structure)
        {
            string encodedType = $"{structure.GetStructureName()}(";

            string nameTypeConcatenated = structure.GetMemberProperties()
                .Aggregate(string.Empty, (accumulated, prop) =>
            {
                string prefix = accumulated == string.Empty ? string.Empty : ",";
                // TODO: Make property name encoding configurable
                string nameType = $"{prefix}{prop.Item2.AbiType} {prop.Item1.Name.ToCamelCase()}";

                return accumulated + nameType;
            });

            encodedType += $"{nameTypeConcatenated})";

            return encodedType;
        }

        private static Tuple<string, string>[] FindTupleTypes<T>(T structure)
        {
            List<Tuple<string, string>> list = new List<Tuple<string, string>>();
            Tuple<PropertyInfo, MemberAttribute>[] props = structure.GetMemberProperties();
            IEnumerable<Tuple<PropertyInfo,MemberAttribute>> tupleProps 
                = props.Where(prop => prop.Item2.AbiType == "tuple");

            foreach (var prop in tupleProps)
            {
                object val = prop.Item1.GetValue(structure);
                string typeName = val.GetStructureName();
                string encodedType = EncodeType(val);

                if(list.Find(t => t.Item1 == typeName) != null)
                    list.Add(new Tuple<string, string>(typeName, encodedType));
                list.AddRange(FindTupleTypes(val));
            }

            return list.ToArray();
        }

        #endregion

    }
}
