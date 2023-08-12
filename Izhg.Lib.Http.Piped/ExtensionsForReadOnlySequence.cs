using System;
using System.Buffers;
using IziHardGames.Core;

namespace IziHardGames.Libs.ForHttp.Piped
{
    public static class ExtensionsForReadOnlySequence
    {
        public static SequencePosition FindPosAfterEndOfHeaders(in this ReadOnlySequence<byte> data)
        {
            ReadOnlySpan<byte> ethalon = stackalloc byte[] { (byte)'\r', (byte)'\n', (byte)'\r', (byte)'\n' };
            Span<byte> span = stackalloc byte[4];
            int spanIndex = default;
            int position = default;

            foreach (var seq in data)
            {
                var seg = seq.Span;
                for (int i = 0; i < seg.Length; i++)
                {
                    if (ethalon[spanIndex] != seg[i])
                    {
                        spanIndex = default;
                        if (ethalon[0] == seg[i]) spanIndex++;
                    }
                    else
                    {
                        spanIndex++;
                    }
                    position++;
                    if (spanIndex == ethalon.Length) return data.GetPosition(position);
                }
            }
            return new SequencePosition(null, -1);
        }

        public static SequencePosition FindPosEndOfHeaders(in this ReadOnlySequence<byte> data)
        {
            ReadOnlySpan<byte> ethalon = stackalloc byte[] { (byte)'\r', (byte)'\n', (byte)'\r', (byte)'\n' };
            Span<byte> span = stackalloc byte[4];
            int spanIndex = default;
            int position = default;

            foreach (var seq in data)
            {
                var seg = seq.Span;
                for (int i = 0; i < seg.Length; i++, position++)
                {
                    if (ethalon[spanIndex] != seg[i])
                    {
                        spanIndex = default;
                        if (ethalon[0] == seg[i]) spanIndex++;
                    }
                    else
                    {
                        spanIndex++;
                    }
                    if (spanIndex == ethalon.Length) return data.GetPosition(position);
                }
            }
            return new SequencePosition(null, -1);
        }
    }
}