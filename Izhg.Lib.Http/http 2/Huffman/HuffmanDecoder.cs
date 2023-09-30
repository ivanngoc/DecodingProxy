﻿using System;
using System.IO;

namespace IziHardGames.Libs.ForHttp20.Huffman
{
    /// <summary>
    /// The HuffmanDecoder class
    /// </summary>
    public class HuffmanDecoder
    {
        private readonly Node root = null;

        /// <summary>
        /// Creates a new Huffman decoder with the specified Huffman coding.
        /// </summary>
        /// <param name="codes">the Huffman codes indexed by symbol</param>
        /// <param name="lengths">the length of each Huffman code</param>
        public HuffmanDecoder(int[] codes, byte[] lengths)
        {
            if (codes.Length != 257 || codes.Length != lengths.Length)
            {
                throw new ArgumentException("invalid Huffman coding");
            }
            this.root = BuildTree(codes, lengths);
        }

        /// <summary>
        /// Decompresses the given Huffman coded string literal.
        /// </summary>
        /// <param name="buf">the string literal to be decoded</param>
        /// <returns>the output stream for the compressed data</returns>
        /// <exception cref="IOException">throws IOException if an I/O error occurs. In particular, an <code>IOException</code> may be thrown if the output stream has been closed.</exception>
        public byte[] Decode(byte[] buf)
        {
            return Decode(new ReadOnlySpan<byte>(buf));
        }
        /// <summary>
        /// Decompresses the given Huffman coded string literal.
        /// </summary>
        /// <param name="buf">the string literal to be decoded</param>
        /// <returns>the output stream for the compressed data</returns>
        /// <exception cref="IOException">throws IOException if an I/O error occurs. In particular, an <code>IOException</code> may be thrown if the output stream has been closed.</exception>
        public byte[] Decode(ReadOnlySpan<byte> buf)
        {
            using var baos = new MemoryStream();
            var node = this.root;
            var current = 0;
            var bits = 0;
            for (var i = 0; i < buf.Length; i++)
            {
                var b = buf[i] & 0xFF;
                current = (current << 8) | b;
                bits += 8;
                while (bits >= 8)
                {
                    var c = (current >> (bits - 8)) & 0xFF;
                    node = node.Children[c];
                    bits -= node.Bits;
                    if (node.IsTerminal() && node.Symbol == HuffmanCoding.HUFFMAN_EOS)
                    {
                        throw new IOException("EOS Decoded");
                    }

                    if (node.IsTerminal())
                    {
                        baos.Write(new byte[] { (byte)node.Symbol }, 0, 1);
                        node = this.root;
                    }
                }
            }

            while (bits > 0)
            {
                var c = (current << (8 - bits)) & 0xFF;
                node = node.Children[c];
                if (node.IsTerminal() && node.Bits <= bits)
                {
                    bits -= node.Bits;
                    baos.Write(new byte[] { (byte)node.Symbol }, 0, 1);
                    node = this.root;
                    continue;
                }

                break;
            }

            // Section 5.2. String Literal Representation
            // Padding not corresponding to the most significant bits of the code
            // for the EOS symbol (0xFF) MUST be treated as a decoding error.
            var mask = (1 << bits) - 1;
            if ((current & mask) != mask)
            {
                throw new IOException("Invalid Padding");
            }

            return baos.ToArray();
        }

        /// <summary>
        /// The Node class
        /// </summary>
        public class Node
        {
            private readonly int symbol;
            // terminal nodes have a symbol
            private readonly int bits;
            // number of bits matched by the node
            private readonly Node[] children;
            // internal nodes have children

            /// <summary>
            /// The Symbol.
            /// </summary>
            public int Symbol { get { return this.symbol; } }

            /// <summary>
            /// The Bits.
            /// </summary>
            public int Bits { get { return this.bits; } }

            /// <summary>
            /// The Children.
            /// </summary>
            public Node[] Children { get { return this.children; } }

            /// <summary>
            /// Initializes a new instance of the HuffmanDecoder.Node class.
            /// </summary>
            public Node()
            {
                this.symbol = 0;
                this.bits = 8;
                this.children = new Node[256];
            }

            /// <summary>
            /// Initializes a new instance of the HuffmanDecoder.Node class.
            /// </summary>
            /// <param name="symbol">the symbol the node represents</param>
            /// <param name="bits">the number of bits matched by this node</param>
            public Node(int symbol, int bits)
            {
                this.symbol = symbol;
                this.bits = bits;
                this.children = null;
            }

            /// <summary>
            /// ???
            /// </summary>
            /// <returns>bool</returns>
            public bool IsTerminal()
            {
                return this.children == null;
            }
        }

        private static Node BuildTree(int[] codes, byte[] lengths)
        {
            var node = new Node();
            for (var i = 0; i < codes.Length; i++)
            {
                Insert(node, i, codes[i], lengths[i]);
            }
            return node;
        }

        private static void Insert(Node root, int symbol, int code, byte length)
        {
            // traverse tree using the most significant bytes of code
            var current = root;
            while (length > 8)
            {
                if (current.IsTerminal())
                {
                    throw new InvalidDataException("invalid Huffman code: prefix not unique");
                }
                length -= 8;
                var i = (code >> length) & 0xFF;
                if (current.Children[i] == null)
                {
                    current.Children[i] = new Node();
                }
                current = current.Children[i];
            }

            var terminal = new Node(symbol, length);
            var shift = 8 - length;
            var start = (code << shift) & 0xFF;
            var end = 1 << shift;
            for (var i = start; i < start + end; i++)
            {
                current.Children[i] = terminal;
            }
        }
    }
}
