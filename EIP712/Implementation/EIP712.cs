
namespace EIP712
{
    public static class EIP712
    {
        public static byte[] Encode<T>(T structure, EIP712Domain domain) where T : class
        {
            byte[] domainSeparator = HashStruct(domain);
            byte[] hash = HashStruct(structure);
            byte[] result = ByteUtil.Merge(Header, domainSeparator, hash);
            return result;
        }
    }
}
