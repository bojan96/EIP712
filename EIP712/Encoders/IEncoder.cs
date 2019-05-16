
namespace EIP712.Encoders
{
    internal interface IEncoder
    {
        byte[] Encode(object val);
    }
}
