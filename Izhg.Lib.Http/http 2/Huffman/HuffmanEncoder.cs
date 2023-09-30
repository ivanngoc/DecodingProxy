﻿using System;
using System.IO;

namespace IziHardGames.Libs.ForHttp20.Huffman
{
	/// <summary>
	/// The HuffmanEncoder class.
	/// </summary>
	public class HuffmanEncoder
	{
		private readonly int[] codes;
		private readonly byte[] lengths;

		/// <summary>
		/// Creates a new Huffman encoder with the specified Huffman coding.
		/// </summary>
		/// <param name="codes">the Huffman codes indexed by symbol</param>
		/// <param name="lengths">the length of each Huffman code</param>
		public HuffmanEncoder(int[] codes, byte[] lengths)
		{
			this.codes = codes;
			this.lengths = lengths;
		}

		/// <summary>
		/// Compresses the input string literal using the Huffman coding.
		/// </summary>
		/// <param name="output">the output stream for the compressed data</param>
		/// <param name="data">the string literal to be Huffman encoded</param>
		/// <exception cref="IOException">if an I/O error occurs.</exception>
		public void Encode(BinaryWriter output, byte[] data)
		{
			this.Encode(output, data, 0, data.Length);
		}

		/// <summary>
		/// Compresses the input string literal using the Huffman coding.
		/// </summary>
		/// <param name="output">the output stream for the compressed data</param>
		/// <param name="data">the string literal to be Huffman encoded</param>
		/// <param name="off">the start offset in the data</param>
		/// <param name="len">the number of bytes to encode</param>
		/// <exception cref="IOException">if an I/O error occurs. In particular, an <code>IOException</code> may be thrown if the output stream has been closed.</exception>
		public void Encode(BinaryWriter output, byte[] data, int off, int len)
		{
			if (output == null)
			{
				throw new NullReferenceException("out");
			}
			else if (data == null)
			{
				throw new NullReferenceException("data");
			}
			else if (off < 0 || len < 0 || off > data.Length || (off + len) > data.Length)
			{
				throw new NullReferenceException();
			}
			else if (len == 0)
			{
				return;
			}

			var current = 0L;
			var n = 0;

			for (var i = 0; i < len; i++)
			{
				var b = data[off + i] & 0xFF;
				var code = (uint)this.codes[b];
				var nbits = (int)lengths[b];

				current <<= nbits;
				current |= code;
				n += nbits;

				while (n >= 8)
				{
					n -= 8;
					output.Write(((byte)(current >> n)));
				}
			}

			if (n > 0)
			{
				current <<= (8 - n);
				current |= (uint)(0xFF >> n); // this should be EOS symbol
				output.Write((byte)current);
			}
		}

		/// <summary>
		/// Returns the number of bytes required to Huffman encode the input string literal.
		/// </summary>
		/// <returns>the number of bytes required to Huffman encode <code>data</code></returns>
		/// <param name="data">the string literal to be Huffman encoded</param>
		public int GetEncodedLength(byte[] data)
		{
			if (data == null)
			{
				throw new NullReferenceException("data");
			}
			var len = 0L;
			foreach (var b in data)
			{
				len += lengths[b & 0xFF];
			}
			return (int)((len + 7) >> 3);
		}
	}
}
