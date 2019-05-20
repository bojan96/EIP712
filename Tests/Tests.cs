using EIP712;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nethereum.Hex.HexConvertors.Extensions;
using System.Linq;
using System.Numerics;

namespace Tests
{
    [TestClass]
    public class Tests
    {

        private const string ZeroAddress = "0x0000000000000000000000000000000000000000";
        private const string PrivateKey = "0xb5b1870957d373ef0eeffecc6e4812c0fd08f554b37b233526acc331bf1544f7";

        [TestMethod()]
        public void AddressTypeSign()
        {
            byte[] signature = EIP712Service.Sign(new TestType { AddressType = ZeroAddress }, 
                new EIP712Domain(), PrivateKey).Packed;

            byte[] expectedSignature = ("0x5a89436f9fa59b0afe4d4bcb0a105c3f2bcc1d36d927ee6060f1f4078ab" +
                "e95593e20f13b16d0850903656bfcf42b83cedc66787e81d573e582dadbf56a3b81af1b")
                .HexToByteArray();

            CollectionAssert.AreEqual(expectedSignature, signature);
        }

        [TestMethod]
        public void StringTypeSign()
        {
            byte[] signature = EIP712Service.Sign(new TestType { StringType = "EthereumMessage" },
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
        public void EmptyTypeSign()
        {
            byte[] signature = EIP712Service.Sign(new TestType(), new EIP712Domain(), PrivateKey).Packed;

            byte[] expectedSignature = ("0x79817d9680ab164e7d009280716814ad2ebdb2af577e3be286ace8f0774cd50" +
                "350ec4ef3af398314e8b334b6d617da53e8eaa98cfc5aa825d0925673ce55d98a1b").
                HexToByteArray();

            CollectionAssert.AreEqual(expectedSignature, signature);
        }

        [TestMethod]
        public void MultipleTypesSign()
        {
            byte[] signature = EIP712Service.Sign(new TestType
            {
                StringType = "test",
                AddressType = ZeroAddress,
                IntegerType = 0,
                BoolType = true,
                BytesType = new byte[1],
                Bytes16Type = new byte[16],
                TupleType = new NestedType
                {
                    StringType = "test"
                }
            }, new EIP712Domain()
            {
                Name = "Test domain name",
                Version = "1",
                ChainId = 3,
                Salt = new byte[32],
                VerifyingContract = ZeroAddress
            }, PrivateKey).Packed;

            byte[] expectedSignature 
                = ("0x9c89f5f9fa10ec8484edb9c627a888abece2de1e8500c90839" +
                "c83be4fbc0fd773706e2e10c451e197a5262a7c9e97c2ce53b21a18" +
                "9d1cca5c11e068fcafff3d41b").HexToByteArray();

            CollectionAssert.AreEqual(expectedSignature, signature);
        }

        [TestMethod]
        public void LargeIntegerSign()
        {
            // Max uint256 value
            byte[] maxUint256Bytes = Enumerable.Repeat(0xff, 33).
                Select(@byte => (byte)@byte).ToArray();
            maxUint256Bytes[32] = 0;

            BigInteger maxUint256 = new BigInteger(maxUint256Bytes);
            byte[] signature = EIP712Service.Sign(
                new TestType
                {
                    IntegerType = maxUint256
                },
                new EIP712Domain(),
                PrivateKey).Packed;

            byte[] expectedSignature = ("0xf3ff9939b98ee1f904707ed4964dea767c" +
                "f02426da9817c3197a083e80704ccf3b52f63bd1412d954cb055fe9334cd7e" +
                "5ef31ba1a3edb06a1f768bdec937812b1c").HexToByteArray();

            CollectionAssert.AreEqual(expectedSignature, signature);
        }

        [TestMethod]
        public void BoolTypeSign()
        {
            byte[] signature = EIP712Service.Sign(new TestType
            {
                BoolType = true
            }, new EIP712Domain(), PrivateKey).Packed;

            byte[] expectedSignature = ("0x20f8ffb5e0ca822597b62f63905d62dac1" +
                "18245de814ca0d25d910b38ed61c14566c7f212dfeb04fc043219f0ec3e3ec" +
                "75cfc10f590a8333faaa04643f3c6e4e1c").HexToByteArray();

            CollectionAssert.AreEqual(expectedSignature, signature);
        }

        [TestMethod]
        public void BytesTypeSign()
        {
            byte[] signature = EIP712Service.Sign(new TestType
            {
                BytesType = new byte[1]
            }, new EIP712Domain(), PrivateKey).Packed;

            byte[] expectedSignature = ("0xe7d7982b3650fa361c47b9758a5e0f8a8644a7" +
                "11fc84be5e138924d24e21430237f82e45a462c64f9187f8efef69635916df17" +
                "94a3afcd2cec7fa5b687d1c3e41c").HexToByteArray();

            CollectionAssert.AreEqual(expectedSignature, signature);
        }

        [TestMethod]
        public void Bytes16Sign()
        {

            byte[] signature = EIP712Service.Sign(
                new TestType
                {
                    Bytes16Type = Enumerable.Repeat<byte>(0xff, 16).ToArray()
                }, new EIP712Domain(), PrivateKey).Packed;

            byte[] expectedSignature = ("0xbace47bbd339880de51139dccf245d29fc6ded7" +
                "5b8216f22f773f2226568a8a45c85c949a6f6a59f356c66c657c12c240cf9fdac" +
                "9b997362e2ed8069439f09c61c").HexToByteArray();

            CollectionAssert.AreEqual(expectedSignature, signature);
        }

        [TestMethod]
        public void TupleSign()
        {
            byte[] signature = EIP712Service.Sign(
                new TestType
                {
                    StringType = "testString",
                    TupleType = new NestedType
                    {
                        StringType = "testString"
                    }
                }, new EIP712Domain(), PrivateKey).Packed;

            byte[] expectedSignature
                = ("0x0b6b1a9ea0715e1f78a29b3b38589fd4e9f01c0344817" +
                "a5c1c67e96565ec3f4c722f84aec1fd8f340b9628573fdcd7b" +
                "abc44f70928be22242991c61d860d19d01c").HexToByteArray();

            CollectionAssert.AreEqual(expectedSignature, signature);
        }
    }
}
