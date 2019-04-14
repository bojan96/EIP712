using static EIP712.EIP712;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nethereum.Hex.HexConvertors.Extensions;

namespace Tests
{
    [TestClass]
    public class Tests
    {

        private const string ZeroAddress = "0x0000000000000000000000000000000000000000";
        private const string PrivateKey = "0xb5b1870957d373ef0eeffecc6e4812c0fd08f554b37b233526acc331bf1544f7";

        [TestMethod]
        public void TestSigning()
        {
            byte[] signature = EIP712.EIP712.Sign(new TestType { StringType = "test", AddressType = ZeroAddress }, new EIP712.EIP712Domain()
            {
                Name = "Test domain name",
                Version = "1",
                ChainId = 3,
                VerifyingContract = ZeroAddress
            }, PrivateKey);

            byte[] expectedSignature 
                = ("0x7687371d01daf89bb7fc1adcb91bc30de45d5f6fea2ac73c123c78a2fbe347b65cb3bd4" +
                "9fac48386a09966d451ebc55e84585f71047c67f02f75ef9939a75f591c").HexToByteArray();

            CollectionAssert.AreEqual(expectedSignature, signature);
        }
    }
}
