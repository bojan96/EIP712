using EIP712;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nethereum.Hex.HexConvertors.Extensions;

namespace Tests
{
    [TestClass]
    public class Tests
    {

        private const string ZeroAddress = "0x0000000000000000000000000000000000000000";
        private const string PrivateKey = "0xb5b1870957d373ef0eeffecc6e4812c0fd08f554b37b233526acc331bf1544f7";

        [TestMethod()]
        public void TestAddressType()
        {
            byte[] signature = EIP712.EIP712.Sign(new TestType { AddressType = ZeroAddress }, 
                new EIP712Domain(), PrivateKey).Packed;

            byte[] expectedSignature = ("0x5a89436f9fa59b0afe4d4bcb0a105c3f2bcc1d36d927ee6060f1f4078ab" +
                "e95593e20f13b16d0850903656bfcf42b83cedc66787e81d573e582dadbf56a3b81af1b")
                .HexToByteArray();

            CollectionAssert.AreEqual(expectedSignature, signature);
        }

        [TestMethod]
        public void TestStringType()
        {
            byte[] signature = EIP712.EIP712.Sign(new TestType { StringType = "EthereumMessage" },
                new EIP712Domain(), PrivateKey).Packed;

            byte[] expectedSignature = ("0x90fc559628b5e423eac6e35f5674040a922a9fc5bd9e176" +
                "38a17bf7e46a0048855ae28fd6948eb9a5a11ad68b56c12d3f00e81bdfda027f8b347d24360bbcd921b")
                .HexToByteArray();

            CollectionAssert.AreEqual(expectedSignature, signature);
        }


        /// <summary>
        /// All member values equal to null
        /// </summary>
        [TestMethod]
        public void TestEmptyType()
        {
            byte[] signature = EIP712.EIP712.Sign(new TestType(), new EIP712Domain(), PrivateKey).Packed;

            byte[] expectedSignature = ("0x79817d9680ab164e7d009280716814ad2ebdb2af577e3be286ace8f0774cd50" +
                "350ec4ef3af398314e8b334b6d617da53e8eaa98cfc5aa825d0925673ce55d98a1b").
                HexToByteArray();

            CollectionAssert.AreEqual(expectedSignature, signature);
        }

        [TestMethod]
        public void MultipleTypes()
        {
            byte[] signature = EIP712.EIP712.Sign(new TestType
            {
                StringType = "test",
                AddressType = ZeroAddress,

                // Setting this to zero causes Nethereum ECDSA signature to return S with length of 31
                // More info: https://github.com/Nethereum/Nethereum/issues/541
                IntegerType = 1 
            }, new EIP712.EIP712Domain()
            {
                Name = "Test domain name",
                Version = "1",
                ChainId = 3,
                VerifyingContract = ZeroAddress
            }, PrivateKey).Packed;

            byte[] expectedSignature 
                = ("0xa65a0c515eaa0d7551c1b37019c45999d24ace285c3c27af2c9f5e63777eceb96da455912b8158c9ecd" +
                "1174532e515a4564b04d214659b28bee63242744f93b51b").HexToByteArray();

            CollectionAssert.AreEqual(expectedSignature, signature);
        }


    }
}
