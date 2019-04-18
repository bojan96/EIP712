namespace EIP712.Utilities
{
    internal static class StringExtensions
    {
        public static string ToCamelCase(this string str)
            => new string(new char[] { str[0] }).ToLowerInvariant() + str.Substring(1);
    }
}
