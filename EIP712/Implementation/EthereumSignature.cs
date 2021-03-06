﻿using Nethereum.Util;

namespace EIP712
{
    public class EthereumSignature
    {

        private byte[] _packed = null;

        internal EthereumSignature(byte[] r, byte[] s, byte[] v)
        {
            R = r;
            S = s;
            V = v;
        }

        /// <summary>
        /// R
        /// </summary>
        public byte[] R { get; }

        /// <summary>
        /// S
        /// </summary>
        public byte[] S { get; }

        /// <summary>
        /// Recovery value
        /// </summary>
        public byte[] V { get; }

        /// <summary>
        /// Ethereum Signature in packed format (i.e. R, S, V) concatenated together
        /// </summary>
        /// <returns> Instance of <see cref="EthereumSignature"/></returns>
        public byte[] Packed => _packed = (_packed ?? ByteUtil.Merge(R, S, V));

    }
}
